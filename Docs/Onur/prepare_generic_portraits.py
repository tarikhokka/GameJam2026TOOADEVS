"""Prepare the user's ten JPEGs without redrawing or splitting any characters.

Usage: python prepare_generic_portraits.py SOURCE_DIRECTORY
Dependencies: pillow, numpy, scipy. Run from a Python environment with these installed.
Existing output files must be removed deliberately before rerunning.
"""

import hashlib
import json
from pathlib import Path
import sys

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage


ROOT = Path(__file__).resolve().parents[2]
SOURCE_STEM = "WhatsApp Image 2026-10-17 at 23.16.27"


def prepare(source, slot):
    rgb = np.array(Image.open(source).convert("RGB"))
    values = rgb.astype(np.int16)
    # Only remove near-white pixels reachable from the canvas boundary.
    # Cream clothing, eyes, teeth, plans and helmets stay intact.
    white = (values.min(axis=2) >= 225) & (np.ptp(values, axis=2) <= 22)
    seeds = np.zeros(white.shape, dtype=bool)
    seeds[0, :] = white[0, :]
    seeds[-1, :] = white[-1, :]
    seeds[:, 0] = white[:, 0]
    seeds[:, -1] = white[:, -1]
    background = ndimage.binary_propagation(seeds, mask=white)
    # Visually reviewed empty gaps between curls; the two pairs also have
    # a white gap between their bodies. Restrict enclosed removal to these
    # regions so eyes, teeth, animal fur and light accessories are protected.
    reviewed_regions = {
        1: [(0, 0, 1122, 430), (470, 1200, 510, 1402)],
        2: [(740, 230, 780, 280)],
        3: [(0, 0, 1122, 500)],
        4: [(0, 0, 1122, 600), (620, 1010, 670, 1110)],
        5: [(0, 0, 1122, 300)],
        6: [(0, 0, 1122, 650)],
        7: [(0, 0, 1122, 200)],
        8: [(0, 0, 1122, 300)],
        9: [(0, 0, 1122, 560)],
        10: [(0, 0, 1122, 200)],
    }
    labels, _ = ndimage.label(white & ~background)
    for label, slices in enumerate(ndimage.find_objects(labels), 1):
        if slices is None:
            continue
        y, x = slices
        if any(x.start >= left and y.start >= top and x.stop <= right and y.stop <= bottom
               for left, top, right, bottom in reviewed_regions[slot]):
            background[slices] |= labels[slices] == label
    alpha = np.where(background, 0, 255).astype(np.uint8)
    rgba = np.dstack((rgb, alpha))
    rgba[background, :3] = 0
    image = Image.fromarray(rgba)
    image = image.crop(image.getchannel("A").getbbox())
    scale = min(934 / image.height, 960 / image.width)
    size = (round(image.width * scale), round(image.height * scale))
    image = image.resize(size, Image.Resampling.LANCZOS)
    output = Image.new("RGBA", (1000, 1000))
    output.alpha_composite(image, ((1000 - image.width) // 2, 1000 - image.height))
    return output


def main(source_dir):
    destination = ROOT / "Assets/_Project/Art"
    sources = [source_dir / (SOURCE_STEM + (f" ({i})" if i else "") + ".jpeg") for i in range(10)]
    targets = [destination / f"portrait_g{i:02}.png" for i in range(1, 11)]
    for source, target in zip(sources, targets):
        if not source.is_file():
            raise FileNotFoundError(source)
        if target.exists():
            raise FileExistsError(target)

    manifest = []
    sheet = Image.new("RGB", (1500, 660), "#746d5b")
    draw = ImageDraw.Draw(sheet)
    for i, (source, target) in enumerate(zip(sources, targets), 1):
        output = prepare(source, i)
        output.save(target, optimize=True)
        manifest.append({
            "id": f"g{i:02}", "source": source.name,
            "source_sha256": hashlib.sha256(source.read_bytes()).hexdigest(),
            "output": str(target.relative_to(ROOT)),
            "output_sha256": hashlib.sha256(target.read_bytes()).hexdigest(),
            "group_customer": i in (1, 4),
        })
        thumb = output.resize((300, 300), Image.Resampling.LANCZOS)
        x, y = ((i - 1) % 5) * 300, ((i - 1) // 5) * 330
        sheet.paste(thumb, (x, y), thumb)
        draw.text((x + 12, y + 304), f"g{i:02}" + (" - pair" if i in (1, 4) else ""), fill="white")
        print(target.name, output.getchannel("A").getbbox())
    (Path(__file__).parent / "Generic_portrait_manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2) + "\n")
    sheet.save(Path(__file__).parent / "Generic_portraits_onizleme.png")


if __name__ == "__main__":
    main(Path(sys.argv[1]))
