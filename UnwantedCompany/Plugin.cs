using BepInEx;
using BepInEx.Logging;

namespace UnwantedCompany
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class UnwantedCompany : BaseUnityPlugin
    {
        public const string ModGUID = "jacedadorito.unwantedcompany";

        public const string ModName = "UnwantedCompany";

        public const string ModVersion = "0.0.1";

        private static UnwantedCompany instance;

        public static ManualLogSource logger;

        private void Awake()
        {
            // Plugin startup logic
            instance = this;

            logger = base.Logger;
            Content.Load();
            BaseGameCaches.Init();
            Patches.Load();

            logger.LogInfo(ModName + " loaded!");
        }

    }
}