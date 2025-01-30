using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;
using OWOGame;

namespace MyBhapticsTactsuit
{
    public class OWOSkin
    {
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        private static bool heartBeatIsActive = false;
        private static bool swingLIsActive = false;
        private static bool swingRIsActive = false;
        private static bool swingIsActive = false;
        
        public Dictionary<String, Sensation> FeedbackMap = new Dictionary<String, Sensation>();

        public OWOSkin()
        {
            RegisterAllSensationsFiles();
            InitializeOWO();
        }

        private async void InitializeOWO()
        {
            LOG("Initializing OWO skin");

            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("0"); ;
            OWO.Configure(gameAuth);

            string[] myIPs = getIPsFromFile("OWO_Manual_IP.txt");
            if (myIPs.Length == 0) await OWO.AutoConnect();
            else
            {
                await OWO.Connect(myIPs);
            }

            if (OWO.ConnectionState == ConnectionState.Connected)
            {
                suitDisabled = false;
                LOG("OWO suit connected.");
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

        public string[] getIPsFromFile(string filename)
        {
            List<string> ips = new List<string>();
            string filePath = Directory.GetCurrentDirectory() + "\\Mods\\" + filename;
            if (File.Exists(filePath))
            {
                LOG("Manual IP file found: " + filePath);
                var lines = File.ReadLines(filePath);
                foreach (var line in lines)
                {
                    IPAddress address;
                    if (IPAddress.TryParse(line, out address)) ips.Add(line);
                    else LOG("IP not valid? ---" + line + "---");
                }
            }
            return ips.ToArray();
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

        void RegisterAllSensationsFiles()
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
                catch (Exception e) { LOG(e.ToString()); }

            }

            systemInitialized = true;
        }

        public async Task HeartBeatFuncAsync()
        {
            while (heartBeatIsActive)
            {
                Feel("HeartBeat", 0);
                await Task.Delay(600);
            }
        }

        public async Task SwingFuncAsync()
        {
            string toFeel = "";

            while (swingRIsActive || swingLIsActive)
            {
                if (swingRIsActive)
                    toFeel = "Swing_R";

                if (swingLIsActive)
                    toFeel = "Swing_L";

                if (swingRIsActive && swingLIsActive)
                    toFeel = "Swing_RL";


                Feel(toFeel, 2);
                await Task.Delay(1000);
            }

            swingIsActive = false;
        }

        public void LOG(string logStr)
        {
            #pragma warning disable CS0618 // remove warning that the logger is deprecated
            MelonLogger.Msg(logStr);
            #pragma warning restore CS0618
        }

        public void Feel(String key, int Priority, float intensity = 1.0f, float duration = 1.0f)
        {
            if (FeedbackMap.ContainsKey(key))
            {
                OWO.Send(FeedbackMap[key].WithPriority(Priority));
                LOG("SENSATION: " + key);
            }
            else LOG("Feedback not registered: " + key);
        }

        public void GetItem(bool isRightHand)
        {
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }

            string keyWand = "GetWand" + postfix;

            Feel(keyWand, 0);
        }

        public void Recoil(bool isRightHand, float intensity = 1.0f)
        {
            float duration = 1.0f;
            //var scaleOption = new bHapticsLib.ScaleOption(intensity, duration);
            //var rotationFront = new bHapticsLib.RotationOption(0f, 0f);
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }

            string keyArm = "RecoilArms" + postfix;
            string keyVest = "RecoilVest" + postfix;
            //bHapticsLib.bHapticsManager.PlayRegistered(keyArm, keyArm, scaleOption, rotationFront);
            //bHapticsLib.bHapticsManager.PlayRegistered(keyVest, keyVest, scaleOption, rotationFront);
        }

        public void Block(bool isRight, float intensity = 1.0f)
        {
            float duration = 1.0f;
            //var scaleOption = new bHapticsLib.ScaleOption(intensity, duration);
            //var rotationFront = new bHapticsLib.RotationOption(0f, 0f);
            string postFix = "_L";
            if (isRight) postFix = "_R";
            string keyVest = "BlockVest" + postFix;
            string keyArm = "BlockArm" + postFix;
            //bHapticsLib.bHapticsManager.PlayRegistered(keyArm, keyArm, scaleOption, rotationFront);
            //bHapticsLib.bHapticsManager.PlayRegistered(keyVest, keyVest, scaleOption, rotationFront);
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

        public void StartSwinging(bool isRight)
        {
            if (isRight)
                swingRIsActive = true;

            if (!isRight)
                swingLIsActive = true;


            if (!swingIsActive)
                SwingFuncAsync();

            swingIsActive = true;
        }

        public void StopSwinging(bool isRight)
        {
            if (isRight)
            {
                swingRIsActive = false;
            }
            else
            {
                swingLIsActive = false;
            }
        }

        public bool IsPlaying(String effect)
        {
            return false;
        }

        public void StopAllHapticFeedback()
        {
            StopHeartBeat();
            StopSwinging(true);
            StopSwinging(false);

            OWO.Stop();
        }


    }
}
