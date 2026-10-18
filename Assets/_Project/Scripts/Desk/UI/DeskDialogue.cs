using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CivilJam.Desk
{
    // Uyuşmazlık diyaloğu (Ahmet kararı 17 Ekim ~19:20, memory.md "💬"): inceleme kaşesinden sonra satırlar sırayla oynatılır.
    // Denetçi satırı denetçi balonunda, müteahhit satırı müteahhidin balonunda (harf harf yazı + yazı sesi ClientSceneView'da).
    // Diyalog sürerken tam ekran görünmez katman tıklamaları yakalar: masa kilitli (inceleme, kaşe, sürükleme), tıklama
    // yazan satırı tamamlar, yazı bittiyse sıradakine geçer; tıklanmadan geçilmez (denetçi balonu "devam etmek için tıklayın" der; autoAdvance > 0 ise o kadar sn sonra kendiliğinden geçer).
    // Denetçi balonu Onur'un InspectorBubbleView'u (katmanın çocuğu): balona tıklama onda (yazıyı tamamlar / ContinueRequested),
    // başka yere tıklama bu katmanda; aynı tıklama iki kez işlenmez. Esc (timeScale 0) sürerken bekleme de durur.
    public class DeskDialogue : MonoBehaviour, IPointerClickHandler
    {
        public struct Line
        {
            public bool Inspector; // true: denetçi (oyuncu), false: müteahhit
            public string Text;

            public Line(bool inspector, string text)
            {
                Inspector = inspector;
                Text = text;
            }
        }

        [SerializeField] private Graphic blocker;          // tam ekran, sadece diyalog sürerken raycast alır
        [SerializeField] private ClientSceneView client;
        [SerializeField] private InspectorBubbleView inspectorBubble; // Onur: denetçi balonu (harf harf yazı + yazı sesi kendisinde)
        [Tooltip("Kaşe görünsün diye diyalog bu kadar sn sonra başlar")]
        [SerializeField] private float startDelay = 0.5f;
        [Tooltip("0 = oyuncu tıklayana kadar bekler. > 0: yazı bittikten sonra tıklanmazsa bu kadar sn sonra sıradaki satıra geçilir")]
        [SerializeField] private float autoAdvance = 0f;

        public bool IsPlaying { get; private set; }

        private Coroutine run;
        private bool advance;
        private bool inspectorSpeaking;

        private void Awake()
        {
            SetBlocking(false);
            if (inspectorBubble != null) inspectorBubble.ContinueRequested += () => advance = true;
        }

        // Satırları sırayla oynatır; bitince done (masa kilidi o zaman kalkar). Oynuyorsa öncekini keser.
        public void Play(IReadOnlyList<Line> lines, Action done)
        {
            Stop();
            if (lines == null || lines.Count == 0)
            {
                done?.Invoke();
                return;
            }
            IsPlaying = true;
            SetBlocking(true);
            run = StartCoroutine(Run(lines, done));
        }

        // Yeni müteahhit / masa kapanırken: diyalog hemen biter (done çağrılmaz).
        public void Stop()
        {
            if (run != null) StopCoroutine(run);
            run = null;
            IsPlaying = false;
            SetBlocking(false);
            HideInspector();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsPlaying) return;
            if (SpeakerTyping()) CompleteSpeaker();
            else advance = true;
        }

        private IEnumerator Run(IReadOnlyList<Line> lines, Action done)
        {
            yield return new WaitForSeconds(startDelay);
            foreach (var line in lines)
            {
                inspectorSpeaking = line.Inspector;
                if (line.Inspector) ShowInspector(line.Text);
                else
                {
                    HideInspector();
                    client.Say(line.Text);
                }

                advance = false;
                while (SpeakerTyping() && !advance) yield return null;
                float waited = 0f;
                while (!advance && (autoAdvance <= 0f || waited < autoAdvance))
                {
                    waited += Time.deltaTime;
                    yield return null;
                }
            }
            HideInspector(); // müteahhidin son cümlesi balonunda kalır
            run = null;
            IsPlaying = false;
            SetBlocking(false);
            done?.Invoke();
        }

        private bool SpeakerTyping() => inspectorSpeaking ? InspectorTyping() : client.IsTyping;

        private void CompleteSpeaker()
        {
            if (inspectorSpeaking) CompleteInspector();
            else client.CompleteTyping();
        }

        private void SetBlocking(bool on)
        {
            if (blocker != null) blocker.raycastTarget = on;
        }

        // ---- Denetçi balonu (Onur: Show / Hide / IsTyping / CompleteTyping) ----
        private void ShowInspector(string text)
        {
            if (inspectorBubble != null) inspectorBubble.Show(text);
        }

        private void HideInspector()
        {
            if (inspectorBubble != null) inspectorBubble.Hide();
        }

        private bool InspectorTyping() => inspectorBubble != null && inspectorBubble.IsTyping;

        private void CompleteInspector()
        {
            if (inspectorBubble != null) inspectorBubble.CompleteTyping();
        }
    }
}
