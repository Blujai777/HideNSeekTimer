    using BepInEx;
    using HUD;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Unity;
    using UnityEngine;

namespace hide_and_seek
{
    [BepInPlugin("Blujai.RedNBlue", "HideNSeek", "1.0")]
    public class HideNSeek : BaseUnityPlugin
    {
        private HideTimer hideTimer;
        private bool bDown, nDown, rDown;

        public void OnEnable()
        {
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
        }

        private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            if (IsPostInit) return;
            IsPostInit = true;

            On.RainWorldGame.RawUpdate += Rainworld_RawUpdate;
        }

        private void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig(self, cam);
            hideTimer = new HideTimer(self, self.fContainers[0]);
            self.AddPart(hideTimer);  // Add timer to HUD system
        }

        private void Rainworld_RawUpdate(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig(self, dt);

            if (hideTimer != null)
            {


                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    hideTimer.SetHideTimer();
                    Logger.LogMessage("Hide timer selected.");
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    hideTimer.SetHuntTimer();
                    Logger.LogMessage("Hunt timer selected.");
                }

                if (Input.GetKeyDown(KeyCode.T))
                {
                    hideTimer.ToggleTimer();
                    Logger.LogMessage("Timer toggled.");
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    hideTimer.ResetTimer();
                    Logger.LogMessage("Timer reset.");
                }

            }

            // Use KeyCode for consistency
            bDown = Input.GetKey(KeyCode.B);
            nDown = Input.GetKey(KeyCode.N);
            rDown = Input.GetKey(KeyCode.R);
        }

        public static bool IsPostInit;
    }
    public enum TimerMode
    {
        Hider,
        Hunter,
        None
    }
    public class HideTimer : HUD.HudPart
    {
        private float Readtimer;
        private bool isRunning;
        private TimerMode currentMode = TimerMode.None;  // Track which timer is active
        private FLabel timerLabel;
        private Vector2 pos, lastPos;
        private float fade, lastFade;
        private float HunterTimer, HiderTimer;
        public HideTimer(HUD.HUD hud, FContainer fContainer) : base(hud)
        {
            HiderTimer = 0f;
            HunterTimer = 0f;
            isRunning = false;

            timerLabel = new FLabel("font", FormatTime(0));
            timerLabel.SetPosition(new Vector2(50f, hud.rainWorld.options.ScreenSize.y - 40f));
            timerLabel.scale = 3.0f;
            timerLabel.alignment = FLabelAlignment.Left;
            pos = new Vector2(80f, hud.rainWorld.options.ScreenSize.y - 60f);
            lastPos = pos;

            fContainer.AddChild(timerLabel);
        }
        

        public override void Update()
        {
            base.Update();
            lastPos = pos;

            if (isRunning)
            {
                // Increment and update the timer based on the current mode
                switch (currentMode)
                {
                    case TimerMode.Hider:
                        HiderTimer += Time.deltaTime;
                        Readtimer = HiderTimer;
                        break;
                    case TimerMode.Hunter:
                        HunterTimer += Time.deltaTime;
                        Readtimer = HunterTimer;
                        break;
                }

                // Update the label with the formatted time
                timerLabel.text = FormatTime(Readtimer);
            }
            else
            {
                // Reset the label if no timer is running
                timerLabel.text = FormatTime(Readtimer);
            }
        }
        public Vector2 DrawPos(float timeStacker)
        {
            return Vector2.Lerp(lastPos, pos, timeStacker);
        }
        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);

            // Smooth alpha fading effect
            float alpha = Mathf.Max(0.2f, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastFade, fade, timeStacker)), 1.5f));

            // Align the label and set its position with smooth interpolation
            timerLabel.alignment = FLabelAlignment.Left;
            timerLabel.x = DrawPos(timeStacker).x;
            timerLabel.y = DrawPos(timeStacker).y;
            timerLabel.alpha = 1f;  // Apply the fading effect
        }

        // Format time to MM:SS
        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            int milliseconds = Mathf.FloorToInt((time % 1) * 100);

            return $"{minutes:D2}:{seconds:D2}:{milliseconds:D2}";
        }

        public void SetHuntTimer()
        {
            if (currentMode != TimerMode.Hunter)
            {
                StopTimer();  // Ensure the current timer stops
                currentMode = TimerMode.Hunter;
                Readtimer = HunterTimer;
            }
        }

        public void SetHideTimer()
        {
            if (currentMode != TimerMode.Hider)
            {
                StopTimer();  // Ensure the current timer stops
                currentMode = TimerMode.Hider;
                Readtimer = HiderTimer;
            }
        }
        public void StopTimer() => isRunning = false;

        public void ResetTimer()
        {
            if (currentMode == TimerMode.Hider)
            {
                HiderTimer = 0f;
            }
            else if (currentMode == TimerMode.Hunter)
            {
                HunterTimer = 0f;
            }

            Readtimer = 0f;
            timerLabel.text = FormatTime(0);
        }

        public void ToggleTimer() => isRunning = !isRunning;
    }
}

