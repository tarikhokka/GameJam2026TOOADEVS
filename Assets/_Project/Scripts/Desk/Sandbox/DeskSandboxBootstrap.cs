using UnityEngine;

namespace CivilJam.Desk
{
    // Sandbox_Desk sahnesinde masayı sahte Level 1 verisiyle başlatır; mesai bitince sonucu loglar ve baştan açar.
    public class DeskSandboxBootstrap : MonoBehaviour
    {
        [SerializeField] private DeskScreen screen;

        private void Start()
        {
            screen.Completed += OnCompleted;
            screen.Show(MockDeskData.CreateLevel1());
        }

        private void OnCompleted(DeskResult result)
        {
            Debug.Log(result);
            screen.Show(MockDeskData.CreateLevel1());
        }
    }
}
