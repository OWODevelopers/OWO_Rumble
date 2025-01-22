using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MelonLoader;

namespace MyBhapticsTactsuit
{
    public class TactsuitVR
    {
        /* A class that contains the basic functions for the bhaptics Tactsuit, like:
         * - A Heartbeat function that can be turned on/off
         * - A function to read in and register all .tact patterns in the bHaptics subfolder
         * - A logging hook to output to the Melonloader log
         * - 
         * */
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        // Event to start and stop the heartbeat thread
        private static ManualResetEvent HeartBeat_mrse = new ManualResetEvent(false);
        private static ManualResetEvent SwingR_mrse = new ManualResetEvent(false);
        private static ManualResetEvent SwingL_mrse = new ManualResetEvent(false);
        // dictionary of all feedback patterns found in the bHaptics directory
        public Dictionary<String, FileInfo> FeedbackMap = new Dictionary<String, FileInfo>();

        //private static bHapticsLib.RotationOption defaultRotationOption = new bHapticsLib.RotationOption(0.0f, 0.0f);

        public void HeartBeatFunc()
        {
            while (true)
            {
                // Check if reset event is active
                HeartBeat_mrse.WaitOne();
                //bHapticsLib.bHapticsManager.PlayRegistered("HeartBeat");
                Thread.Sleep(600);
            }
        }
        public void SwingRFunc()
        {
            while (true)
            {
                // Check if reset event is active
                SwingR_mrse.WaitOne();
                //bHapticsLib.bHapticsManager.PlayRegistered("SwingVest_R");
                //bHapticsLib.bHapticsManager.PlayRegistered("SwingArms_R");
                Thread.Sleep(1000);
            }
        }

        public void SwingLFunc()
        {
            while (true)
            {
                // Check if reset event is active
                SwingL_mrse.WaitOne();
                //bHapticsLib.bHapticsManager.PlayRegistered("SwingVest_L");
                //bHapticsLib.bHapticsManager.PlayRegistered("SwingArms_L");
                Thread.Sleep(1000);
            }
        }

        public TactsuitVR()
        {
            LOG("Initializing suit");
            if (!bHaptics.WasError)
            {
                suitDisabled = false;
            }
            RegisterAllTactFiles();
            LOG("Starting HeartBeat thread...");
            Thread HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.Start();
            Thread SwingRThread = new Thread(SwingRFunc);
            SwingRThread.Start();
            Thread SwingLThread = new Thread(SwingLFunc);
            SwingLThread.Start();
        }

        public void LOG(string logStr)
        {
#pragma warning disable CS0618 // remove warning that the logger is deprecated
            MelonLogger.Msg(logStr);
#pragma warning restore CS0618
        }



        void RegisterAllTactFiles()
        {
            // Get location of the compiled assembly and search through "bHaptics" directory and contained patterns
            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\bHaptics";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.tact", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                // LOG("Trying to register: " + prefix + " " + fullName);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    //bHapticsLib.bHapticsManager.RegisterPatternFromJson(prefix, tactFileStr);
                    LOG("Pattern registered: " + prefix);
                }
                catch (Exception e) { LOG(e.ToString()); }

                FeedbackMap.Add(prefix, Files[i]);
            }
            systemInitialized = true;
        }

        public void PlaybackHaptics(String key, float intensity = 1.0f, float duration = 1.0f)
        {
            //LOG("Trying to play");
            if (FeedbackMap.ContainsKey(key))
            {
                //LOG("ScaleOption");
                //bHapticsLib.ScaleOption scaleOption = new bHapticsLib.ScaleOption(intensity, duration);
                //LOG("Submit");
                //bHapticsLib.bHapticsManager.PlayRegistered(key, key, scaleOption, defaultRotationOption);
                // LOG("Playing back: " + key);
            }
            else
            {
                LOG("Feedback not registered: " + key);
            }
        }

        public void PlayBackHit(String key, float xzAngle, float yShift)
        {
            // two parameters can be given to the pattern to move it on the vest:
            // 1. An angle in degrees [0, 360] to turn the pattern to the left
            // 2. A shift [-0.5, 0.5] in y-direction (up and down) to move it up or down
            //bHapticsLib.ScaleOption scaleOption = new bHapticsLib.ScaleOption(1f, 1f);
            //bHapticsLib.RotationOption rotationOption = new bHapticsLib.RotationOption(xzAngle, yShift);
            //bHapticsLib.bHapticsManager.PlayRegistered(key, key, scaleOption, rotationOption);
        }

        public void GetItem(bool isRightHand)
        {
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }

            string keyWand = "GetWand" + postfix;

            PlaybackHaptics(keyWand);
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
            HeartBeat_mrse.Set();
        }

        public void StopHeartBeat()
        {
            HeartBeat_mrse.Reset();
        }
        public void StartSwinging(bool isRightHand)
        {
            if (isRightHand) SwingR_mrse.Set();
            else SwingL_mrse.Set();
        }

        public void StopSwinging(bool isRightHand)
        {
            if (isRightHand)
            {
                SwingR_mrse.Reset();
                StopHapticFeedback("SwingVest_R");
                StopHapticFeedback("SwingArms_R");
            }
            else
            {
                SwingL_mrse.Reset();
                StopHapticFeedback("SwingVest_L");
                StopHapticFeedback("SwingArms_L");
            }
        }

        public bool IsPlaying(String effect)
        {
            return false;
            //return bHapticsLib.bHapticsManager.IsPlaying(effect);
        }

        public void StopHapticFeedback(String effect)
        {
            //bHapticsLib.bHapticsManager.StopPlaying(effect);
        }

        public void StopAllHapticFeedback()
        {
            StopThreads();
            foreach (String key in FeedbackMap.Keys)
            {
                //bHapticsLib.bHapticsManager.StopPlaying(key);
            }
        }

        public void StopThreads()
        {
            // Yes, looks silly here, but if you have several threads like this, this is
            // very useful when the player dies or starts a new level
            StopHeartBeat();
        }


    }
}
