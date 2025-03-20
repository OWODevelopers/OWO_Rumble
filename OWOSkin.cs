using MelonLoader;
using OWOGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Rumble_OWO
{

    public class OWOSkin
    {
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        public bool heartBeatIsActive = false;
        public bool onGuardIsActive = false;


        public Dictionary<String, Sensation> FeedbackMap = new Dictionary<string, Sensation>();

        public OWOSkin()
        {
            RegisterAllSensationsFiles();
            InitializeOWO();
        }

        ~OWOSkin()
        {
            LOG("Destructor called");
            DisconnectOWO();
        }

        public void DisconnectOWO()
        {
            LOG("Disconnecting OWO skin.");
            OWO.Disconnect();
        }

        public void LOG(string logStr)
        {
            MelonLogger.Msg(logStr);
        }


        private void RegisterAllSensationsFiles()
        {
            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\OWO";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.owo", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    Sensation test = Sensation.Parse(tactFileStr);
                    FeedbackMap.Add(prefix, test);
                }
                catch (Exception e) { LOG(e.Message); }

            }

            systemInitialized = true;
        }

        private async void InitializeOWO()
        {
            LOG("Initializing OWO skin");

            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("67884529");

            OWO.Configure(gameAuth);
            string[] myIPs = GetIPsFromFile("OWO_Manual_IP.txt");
            if (myIPs.Length == 0) await OWO.AutoConnect();
            else
            {
                await OWO.Connect(myIPs);
            }

            if (OWO.ConnectionState == OWOGame.ConnectionState.Connected)
            {
                suitDisabled = false;
                LOG("OWO suit connected.");
                Feel("Heart Beat");
            }
            if (suitDisabled) LOG("OWO is not enabled?!?!");
        }

        public BakedSensation[] AllBakedSensations()
        {
            var result = new List<BakedSensation>();

            foreach (var sensation in FeedbackMap.Values)
            {
                if (sensation is BakedSensation baked)
                {
                    LOG("Registered baked sensation: " + baked.name);
                    result.Add(baked);
                }
                else
                {
                    LOG("Sensation not baked? " + sensation);
                    continue;
                }
            }
            return result.ToArray();
        }

        public string[] GetIPsFromFile(string filename)
        {
            List<string> ips = new List<string>();
            string filePath = Directory.GetCurrentDirectory() + "\\BepinEx\\Plugins\\OWO" + filename;
            if (File.Exists(filePath))
            {
                LOG("Manual IP file found: " + filePath);
                var lines = File.ReadLines(filePath);
                foreach (var line in lines)
                {
                    if (IPAddress.TryParse(line, out _)) ips.Add(line);
                    else LOG("IP not valid? ---" + line + "---");
                }
            }
            return ips.ToArray();
        }

        public void Feel(String key, int Priority = 0, float intensity = 1.0f, float duration = 1.0f)
        {
            LOG("SENSATION: " + key);

            if (FeedbackMap.ContainsKey(key))
            {
                OWO.Send(FeedbackMap[key].WithPriority(Priority));
            }

            else LOG("Feedback not registered: " + key);
        }

        #region HeartBeat

        public async Task HeartBeatFuncAsync()
        {
            while (heartBeatIsActive)
            {
                Feel("Heart Beat", 0);
                await Task.Delay(1000);
            }
        }
        public void StartHeartBeat()
        {
            if (heartBeatIsActive) return;

            heartBeatIsActive = true;
            HeartBeatFuncAsync();
        }

        public void StopHeartBeat()
        {
            heartBeatIsActive = false;
        }

        #endregion        
        
        #region OnGuard

        public async Task OnGuardFuncAsync()
        {
            while (onGuardIsActive)
            {
                Feel("Heart Beat", 0);
                await Task.Delay(1000);
            }
        }
        public void StartOnGuard()
        {
            if (onGuardIsActive) return;

            onGuardIsActive = true;
            OnGuardFuncAsync();
        }

        public void StopOnGuard()
        {
            onGuardIsActive = false;
        }

        #endregion

        public void PlayBackHit(String key, float myRotation)
        {
            Sensation hitSensation = Sensation.Parse("100,3,76,0,200,0,Impact");

            if (myRotation >= 0 && myRotation <= 180)
            {
                if (myRotation >= 0 && myRotation <= 90) hitSensation = hitSensation.WithMuscles(Muscle.Dorsal_L, Muscle.Lumbar_L);
                else hitSensation = hitSensation.WithMuscles(Muscle.Dorsal_R, Muscle.Lumbar_R);
            }
            else
            {
                if (myRotation >= 270 && myRotation <= 359) hitSensation = hitSensation.WithMuscles(Muscle.Pectoral_L, Muscle.Abdominal_L);
                else hitSensation.WithMuscles(Muscle.Pectoral_R, Muscle.Abdominal_R);
            }

            if (suitDisabled) { return; }
            OWO.Send(hitSensation.WithPriority(3));

        }

        public void StopAllHapticFeedback()
        {
            StopHeartBeat();

            OWO.Stop();
        }

    }
}
