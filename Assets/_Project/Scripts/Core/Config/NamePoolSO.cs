using System.Collections.Generic;
using UnityEngine;

namespace CivilJam.Core
{
    /// <summary>
    /// Kurgusal isim havuzu: gazetedeki kayıp isimleri, bina adları, adresler.
    /// Hassasiyet: isimler rastgele birleştirilir; gerçek kişi/olay çağrıştıracak tam isim yazılmaz.
    /// </summary>
    [CreateAssetMenu(menuName = "CivilJam/Name Pool", fileName = "NamePool")]
    public class NamePoolSO : ScriptableObject
    {
        public List<string> FirstNames = new List<string>();
        public List<string> LastNames = new List<string>();
        public List<string> BuildingNames = new List<string>();   // "Papatya" → "Papatya Apartmanı"
        public List<string> Neighborhoods = new List<string>();
        public List<string> Streets = new List<string>();
        public List<string> LabNames = new List<string>();
        public List<string> EngineerNames = new List<string>();   // Belgelerdeki imza sahibi
    }
}
