using System;
using System.Collections.Generic;
using System.Text;

// Masa ekranının kendi görünüm modelleri. Core'daki ProjectData/DocumentData/ParamEntry
// repoya gelince tek bir adapter bunlara çevirecek; Desk'in geri kalanı Core'u bilmez.
namespace CivilJam.Desk
{
    public enum DeskDocumentKind { Project, LabReport, Other }

    public enum DeskVerdict { Pending, Approved, Rejected }

    [Serializable]
    public class DeskParam
    {
        public string Name;
        public string Unit;
        public float Required;
        public float Reported;   // belgede yazan
        public float Actual;     // gerçek değer, sadece ek testle görünür
    }

    [Serializable]
    public class DeskDocument
    {
        public string Title;
        public DeskDocumentKind Kind;
        public string ParcelNo;
        public string Date;
        public string Issuer;
        public string Signature;
        public bool HasStamp;
        public List<string> Lines = new List<string>();
        public List<DeskParam> Params = new List<DeskParam>();
    }

    [Serializable]
    public class DeskProject
    {
        public string Id;
        public string Name;
        public string BuildingType;
        public string ClientName;
        public string ClientLine;
        public int BribeAmount;   // 0 = rüşvet teklifi yok
        public List<DeskDocument> Documents = new List<DeskDocument>();

        public bool HasBribe => BribeAmount > 0;
    }

    [Serializable]
    public class DeskSessionConfig
    {
        public string LevelTitle;
        public int StartMoney;
        public int ExtraTestCost;
        public int Quota;
        public List<DeskProject> Projects = new List<DeskProject>();
    }

    [Serializable]
    public class DeskDecision
    {
        public string ProjectId;
        public DeskVerdict Verdict;
        public bool TookBribe;
        public bool ExtraTestBought;
    }

    [Serializable]
    public class DeskResult
    {
        public List<DeskDecision> Decisions = new List<DeskDecision>();
        public int ApprovedCount;
        public int Quota;
        public int ExtraTestSpent;
        public int BribeIncome;
        public int EndMoney;

        public bool QuotaMet => ApprovedCount >= Quota;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[Desk] Onay {ApprovedCount}/{Quota} (kota {(QuotaMet ? "tuttu" : "tutmadı")}), " +
                          $"test gideri {ExtraTestSpent}, rüşvet {BribeIncome}, kasa {EndMoney}");
            foreach (var d in Decisions)
                sb.AppendLine($"  {d.ProjectId}: {d.Verdict}{(d.TookBribe ? " +zarf" : "")}{(d.ExtraTestBought ? " +test" : "")}");
            return sb.ToString();
        }
    }
}
