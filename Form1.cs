﻿using cw;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CW_Beach
{
    public partial class MainForm : Form
    {
        private Thread rapidFireT;
        private Thread instaKillT;
        private Thread namePlayerT;
        private Thread currentWeaponT;
        private Thread tpZombiT;

        private Thread freeze0T;
        private Thread freeze1T;
        private Thread freeze2T;
        private Thread freeze3T;

        //-----------------------------------------------------------------------------------------------------------------------

        public IntPtr PlayerCompPtr, PlayerPedPtr, ZMGlobalBase, ZMBotBase, ZMBotListBase;

        public string currentVersion = "Change adress if CW is not 1.25.3";

        public int gamePID = 0;
        public IntPtr hProc;
        public IntPtr baseAddress = IntPtr.Zero;
        public Process gameProc;

        public Color defaultColor = Color.Black;
        public bool isrunning = false;
        public bool ammoFrozen;
        public bool uneFois = true;

        public Single playerSpeed = -1f;

        public Vector3 frozenPlayerPos = Vector3.Zero;
        public Vector3 lastKnownPlayerPos = Vector3.Zero;
        public Vector3 updatedPlayerPos = Vector3.Zero;
        public Vector3 zombieTpPos;

        public int[] ammoVals = new int[6];
        public int[] maxAmmoVals = new int[6];

        public Single TimesModifier = 1.0f;

        public void consoleOut(string str)
        {
            logsText.Invoke(new MethodInvoker(delegate
            {
                logsText.AppendText(str);
                logsText.AppendText(Environment.NewLine);
            }));
        }

        public void UpdateTextBox(TextBox textbox, string str)
        {
            textbox.Invoke(new MethodInvoker(delegate
            {
                textbox.Clear();
                textbox.AppendText(str);
            }));
        }

        public void UpdateLabel(Label label, string text, string color = "Black")
        {
            if (this.InvokeRequired)
            {
                label.Invoke((MethodInvoker)delegate ()
                {
                    label.Text = text;
                    label.ForeColor = Color.FromName(color);
                });
                return;
            }
            label.Text = text;
            label.ForeColor = Color.FromName(color);
        }

        private void godmodCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (godmodCheck.Checked)
            {
                consoleOut("GODMOD ON");
            }
            else
            {
                consoleOut("GODMOD OFF");
            }
        }

        private void attachButton_Click(object sender, EventArgs e)
        {
            isrunning = !isrunning;

            if (isrunning)
            {
                attachButton.Text = "RUNNING";
                attachButton.ForeColor = Color.Green;
            }
            else
            {
                attachButton.Text = "STOPPED";
                attachButton.ForeColor = Color.Red;
            }

            changeAdressButton.Enabled = false;
            playerBaseTextBox.Enabled = false;
            xpScaleTextBox.Enabled = false;
            cmdBufferTextBox.Enabled = false;
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void rapifFirecheck_CheckedChanged(object sender, EventArgs e)
        {
            if (rapifFirecheck.Checked)
            {
                rapidFireT = new Thread(RapidFire) { IsBackground = true };
                rapidFireT.Start();
            }
            else
            {
                rapidFireT.Abort();
            }
        }

        private void moveSpeedTrackBar_Scroll(object sender, EventArgs e)
        {
            moveSpeedLabel.Text = moveSpeedTrackBar.Value.ToString();

            playerSpeed = moveSpeedTrackBar.Value;
            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_RunSpeed, Convert.ToSingle(playerSpeed), 4, out _);
        }

        private void instaKillCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (instaKillCheck.Checked)
            {
                instaKillT = new Thread(InstaKill) { IsBackground = true };
                instaKillT.Start();
            }
            else
            {
                instaKillT.Abort();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            consoleOut(currentVersion);
            consoleOut("Press \"ok\" for unlock attach.");
            tpZombiT = new Thread(TpZombie) { IsBackground = true };
            tpZombiT.Start();
            if (!backgroundWorker1.IsBusy) backgroundWorker1.RunWorkerAsync();

            playerBaseTextBox.Text = "0x" + AdressOffset.PlayerBase.ToInt64().ToString("X");
            cmdBufferTextBox.Text = "0x" + AdressOffset.CMDBufferBase.ToInt64().ToString("X");
            xpScaleTextBox.Text = "0x" + AdressOffset.XPScaleBase.ToInt64().ToString("X");
        }

        private void thermalScopeCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (thermalScopeCheck.Checked)
            {
                cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_InfraredVision, 0x10, 1, out _);
                logsText.AppendText("THERMAL SCOPE ON (reset if escaping)");
                logsText.AppendText(Environment.NewLine);
            }
            else
            {
                cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_InfraredVision, 0x0, 1, out _);
                logsText.AppendText("THERMAL SCOPE OFF");
                logsText.AppendText(Environment.NewLine);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int c = 0;
            bool verif = false;

            while (true)
            {
                try
                {
                    if (godmodCheck.Enabled && !isrunning)
                    {
                        godmodCheck.Enabled = false;
                        munInfCheck.Enabled = false;
                        moneyInfCheck.Enabled = false;
                        rapifFirecheck.Enabled = false;
                        instaKillCheck.Enabled = false;
                        moveSpeedTrackBar.Enabled = false;
                        thermalScopeCheck.Enabled = false;
                        tpZombiCheck.Enabled = false;
                        changeWeaponButton.Enabled = false;
                        godmodeAllCheck.Enabled = false;
                        munitionInfAllCheck.Enabled = false;
                        tpZombieSavePointCheck.Enabled = false;
                        infMoneyAllCheck.Enabled = false;
                        changeWPP2.Enabled = false;
                        changeWPP3.Enabled = false;
                        changeWPP4.Enabled = false;
                        critKillCheck.Enabled = false;
                        allCritKill.Enabled = false;
                        autoFireCheck.Enabled = false;
                        freeze0Check.Enabled = false;
                        freeze1Check.Enabled = false;
                        freeze2Check.Enabled = false;
                        freeze3Check.Enabled = false;
                        cmdBufferInput.Enabled = false;
                        cmdBufferBtn.Enabled = false;
                        kick2.Enabled = false;
                        Kick3.Enabled = false;
                        Kick4.Enabled = false;

                        //activeXPCheck.Enabled = false;

                        godmodCheck.Checked = false;
                        munInfCheck.Checked = false;
                        moneyInfCheck.Checked = false;
                        rapifFirecheck.Checked = false;
                        instaKillCheck.Checked = false;
                        moveSpeedTrackBar.Value = 1;
                        thermalScopeCheck.Checked = false;
                        tpZombiCheck.Checked = false;
                        godmodeAllCheck.Checked = false;
                        munitionInfAllCheck.Checked = false;
                        tpZombieSavePointCheck.Checked = false;
                        infMoneyAllCheck.Checked = false;
                        critKillCheck.Checked = false;
                        allCritKill.Checked = false;
                        autoFireCheck.Checked = false;
                        freeze0Check.Checked = false;
                        freeze1Check.Checked = false;
                        freeze2Check.Checked = false;
                        freeze3Check.Checked = false;

                        //activeXPCheck.Checked = false;
                    }

                    if (!isrunning)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    var gameProcs = Process.GetProcessesByName("BlackOpsColdWar");

                    // if there aren't any processes, update the game message label and do nothing
                    if (gameProcs.Length < 1)
                    {
                        consoleOut("GAME NOT RUNNING !");
                        Thread.Sleep(100);
                        continue;
                    }

                    // get first process from the gameProcs array
                    gameProc = gameProcs[0];

                    gamePID = gameProc.Id;

                    if (gamePID < 1)
                    {
                        consoleOut("Game is not running");
                        Thread.Sleep(100);
                        continue;
                    }

                    // opens the process or something, not 100% still learning all this terminology
                    hProc = cwapi.OpenProcess(cwapi.ProcessAccessFlags.All, false, gameProc.Id);

                    // if the base address isn't uptodate, update it
                    if (baseAddress != cwapi.GetModuleBaseAddress(gameProc, "BlackOpsColdWar.exe"))
                    {
                        baseAddress = cwapi.GetModuleBaseAddress(gameProc, "BlackOpsColdWar.exe");
                        c++;
                        consoleOut($"Adresse catch ({c}/6)");
                    }

                    // cache the base addresses for these various pointers
                    if (PlayerCompPtr != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64()), new int[] { 0 }))
                    {
                        PlayerCompPtr = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64()), new int[] { 0 });
                        c++;
                        consoleOut($"Adresse catch ({c}/6)");
                    }

                    if (PlayerPedPtr != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64() + 0x8), new int[] { 0 }))
                    {
                        PlayerPedPtr = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64() + 0x8), new int[] { 0 });
                        c++;
                        consoleOut($"Adresse catch ({c}/6)");
                    }

                    if (ZMGlobalBase != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64() + 0x60), new int[] { 0 }))
                    {
                        ZMGlobalBase = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64() + 0x60), new int[] { 0 });
                        c++;
                        consoleOut($"Adresse catch ({c}/6)");
                    }

                    if (ZMBotBase != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64() + 0x68), new int[] { 0 }))
                    {
                        ZMBotBase = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64()) + 0x68, new int[] { 0 });
                        c++;
                        consoleOut($"Adresse catch ({c}/6)");
                    }

                    if (ZMBotBase != (IntPtr)0x0 && ZMBotBase != (IntPtr)0x68 && ZMBotListBase != cwapi.FindDMAAddy(hProc, ZMBotBase + AdressOffset.ZM_Bot_List_Offset, new int[] { 0 }))
                    {
                        ZMBotListBase = cwapi.FindDMAAddy(hProc, ZMBotBase + AdressOffset.ZM_Bot_List_Offset, new int[] { 0 });
                        c++;
                        consoleOut($"Adresse catch ({c}/6)");
                    }

                    if (!verif && c != 6)
                    {
                        verif = true;
                        consoleOut("CARE ALL FEATURE DON'T WORK!!!");
                    }

                    if (godmodeAllCheck.Checked | godmodCheck.Checked)
                    {
                        if (godmodCheck.Checked)
                        {
                            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_GodMode, 0xA0, 1, out _);
                        }
                        if (godmodeAllCheck.Checked)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                cwapi.WriteProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * i) + AdressOffset.PC_GodMode, 0xA0, 1, out _);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * i) + AdressOffset.PC_GodMode, 0x20, 1, out _);
                        }
                    }

                    if (critKillCheck.Checked | allCritKill.Checked)
                    {
                        if (critKillCheck.Checked)
                        {
                            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.CritKill8, -1, 1, out _);
                        }
                        if (allCritKill.Checked)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                cwapi.WriteProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * i) + AdressOffset.CritKill8, -1, 1, out _);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * i) + AdressOffset.CritKill8, 0, 1, out _);
                        }
                    }

                    if (munInfCheck.Checked)
                    {
                        for (int i = 1; i < 6; i++)
                        {
                            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_Ammo + (i * 0x4), 100, 4, out _);
                        }
                    }

                    if (munitionInfAllCheck.Checked)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            for (int j = 1; j < 6; j++)
                            {
                                cwapi.WriteProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * i) + AdressOffset.PC_Ammo + (j * 0x4), 100, 4, out _);
                            }
                        }
                    }

                    if (infMoneyAllCheck.Checked)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * i) + AdressOffset.PC_Points, 8000000, 4, out _);
                        }
                    }

                    if (moneyInfCheck.Checked)
                    {
                        cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_Points, 8000000, 4, out _);
                    }

                    if (autoFireCheck.Checked)
                    {
                        cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_autoFire, 1, 1, out _);
                    }

                    if (uneFois)
                    {
                        namePlayerT = new Thread(StatPlayerGrab) { IsBackground = true };
                        namePlayerT.Start();
                        currentWeaponT = new Thread(CurrentWeapon) { IsBackground = true };
                        currentWeaponT.Start();

                        uneFois = false;
                    }

                    if (!godmodCheck.Enabled)
                    {
                        godmodCheck.Enabled = true;
                        munInfCheck.Enabled = true;
                        moneyInfCheck.Enabled = true;
                        rapifFirecheck.Enabled = true;
                        instaKillCheck.Enabled = true;
                        moveSpeedTrackBar.Enabled = true;
                        thermalScopeCheck.Enabled = true;
                        tpZombiCheck.Enabled = true;
                        changeWeaponButton.Enabled = true;
                        godmodeAllCheck.Enabled = true;
                        munitionInfAllCheck.Enabled = true;
                        tpZombieSavePointCheck.Enabled = true;
                        infMoneyAllCheck.Enabled = true;
                        changeWPP2.Enabled = true;
                        changeWPP3.Enabled = true;
                        changeWPP4.Enabled = true;
                        critKillCheck.Enabled = true;
                        allCritKill.Enabled = true;
                        autoFireCheck.Enabled = true;

                        //activeXPCheck.Enabled = true;

                        cmdBufferInput.Enabled = true;
                        cmdBufferBtn.Enabled = true;
                        kick2.Enabled = true;
                        Kick3.Enabled = true;
                        Kick4.Enabled = true;

                        freeze0Check.Enabled = true;
                        freeze1Check.Enabled = true;
                        freeze2Check.Enabled = true;
                        freeze3Check.Enabled = true;
                    }
                }
                catch (Exception err)
                {
                    consoleOut(err.Message);
                }
                Thread.Sleep(40);
            }
        }

        private void tpZombiCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (tpZombiCheck.Checked)
            {
                tpZombieSavePointCheck.Checked = false;
            }
        }

        private void tpZombieSavePointCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (tpZombieSavePointCheck.Checked)
            {
                tpZombiCheck.Checked = false;
            }
        }

        

        private void xpPlayerBar_Scroll(object sender, EventArgs e)
        {
            /*byte[] tBuff = new byte[4];
            Buffer.BlockCopy(BitConverter.GetBytes((float)xpPlayerBar.Value), 0, tBuff, 0, 4);
            xpPlayerLabel.Text = BitConverter.ToSingle(tBuff, 0).ToString();
            cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + XPScaleBase.ToInt64()) + PlayerXP, tBuff, 4, out _);
            cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + XPScaleBase.ToInt64()) + PlayerXP2, tBuff, 4, out _);
            cwapi.ReadProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + XPScaleBase.ToInt64()) + PlayerXP, tBuff, 4, out _);
            xpWeaponLabel.Text = BitConverter.ToSingle(tBuff, 0).ToString();*/
        }

        private void xpWeaponBar_Scroll(object sender, EventArgs e)
        {
            /*byte[] tBuff = new byte[4];
            Buffer.BlockCopy(BitConverter.GetBytes((float)xpWeaponBar.Value), 0, tBuff, 0, 4);
            cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + XPScaleBase.ToInt64()) + WeaponXP, tBuff, 4, out _);
            cwapi.ReadProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + XPScaleBase.ToInt64()) + WeaponXP, tBuff, 4, out _);
            xpWeaponLabel.Text = BitConverter.ToSingle(tBuff, 0).ToString();*/
        }

        private void setWeaponText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '-'))
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.unknowncheats.me/forum/3265024-post2151.html");
        }

        private void godmodeAllCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (godmodeAllCheck.Checked)
            {
                consoleOut("GODMOD ALL ON");
            }
            else
            {
                consoleOut("GODMOD ALL OFF");
            }
        }

        private void xpWeaponLabel_Click(object sender, EventArgs e)
        {
        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {
        }

        private void player3_Click(object sender, EventArgs e)
        {
        }

        private void changeWPP2_Click(object sender, EventArgs e)
        {
            long x = long.Parse(wpP2Text.Text);
            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_ArraySize_Offset + AdressOffset.PC_SetWeaponID, x, 8, out _);
        }

        private void changeWPP3_Click(object sender, EventArgs e)
        {
            long x = long.Parse(wpP3Text.Text);
            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * 2) + AdressOffset.PC_SetWeaponID, x, 8, out _);
        }

        private void changeWPP4_Click(object sender, EventArgs e)
        {
            long x = long.Parse(wpP4Text.Text);
            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * 3) + AdressOffset.PC_SetWeaponID, x, 8, out _);
        }

        private void changeWeaponButton_Click(object sender, EventArgs e)
        {
            long x = long.Parse(setWeaponText.Text);
            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_SetWeaponID, x, 8, out _);
        }

        private void autoFireCheck_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void distanceTPBar_Scroll(object sender, EventArgs e)
        {
            distanceTPLabel.Text = distanceTPBar.Value.ToString();
        }

        private void critKillCheck_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void kick2_Click(object sender, EventArgs e)
        {
            CmdBufferExec("clientkick 1");
        }

        private void killLobbyBtn_Click(object sender, EventArgs e)
        {
            attachGame();
            CmdBufferExec("xstoppartykeeptogether");
            CmdBufferExec("hostmigration_start");
            CmdBufferExec("killserver");
            CmdBufferExec("disconnect");
        }

        private void Kick3_Click(object sender, EventArgs e)
        {
            CmdBufferExec("clientkick 2");
        }

        private void Kick4_Click(object sender, EventArgs e)
        {
            CmdBufferExec("clientkick 3");
        }

        private void instantSartBtn_Click(object sender, EventArgs e)
        {
            attachGame();
            for (int i = 0; i < 3; i++)
            {
                CmdBufferExec("LobbyLaunchGame");
            }
        }

        private void cmdBufferBtn_Click(object sender, EventArgs e)
        {
            CmdBufferExec(cmdBufferInput.Text);
        }

        private void freeze0Check_CheckedChanged(object sender, EventArgs e)
        {
            if (freeze0Check.Checked)
            {
                freeze0T = new Thread(freezePlayer) { IsBackground = true };
                freeze0T.Start(0);
            }
            else
            {
                freeze0T.Abort();
            }
        }

        private void freeze1Check_CheckedChanged(object sender, EventArgs e)
        {
            if (freeze1Check.Checked)
            {
                freeze1T = new Thread(freezePlayer) { IsBackground = true };
                freeze1T.Start(1);
            }
            else
            {
                freeze1T.Abort();
            }
        }

        private void freeze2Check_CheckedChanged(object sender, EventArgs e)
        {
            if (freeze2Check.Checked)
            {
                freeze2T = new Thread(freezePlayer) { IsBackground = true };
                freeze2T.Start(2);
            }
            else
            {
                freeze2T.Abort();
            }
        }

        private void freeze3Check_CheckedChanged(object sender, EventArgs e)
        {
            if (freeze3Check.Checked)
            {
                freeze3T = new Thread(freezePlayer) { IsBackground = true };
                freeze3T.Start(3);
            }
            else
            {
                freeze3T.Abort();
            }
        }

        private void topMostButton_CheckedChanged(object sender, EventArgs e)
        {
            if (topMostButton.Checked)
            {
                TopMost = true;
            }
            else
            {
                TopMost = true;
            }
        }

        private void changeAdressButton_Click(object sender, EventArgs e)
        {
            long pbTmp, cmdTmp, xpTmp;
            if (playerBaseTextBox.Text.StartsWith("0x"))
            {
                pbTmp = long.Parse(playerBaseTextBox.Text.Substring(2), System.Globalization.NumberStyles.HexNumber);
                AdressOffset.PlayerBase = new IntPtr(pbTmp);
            }
            else
            {
                pbTmp = long.Parse(playerBaseTextBox.Text, System.Globalization.NumberStyles.HexNumber);
                AdressOffset.PlayerBase = new IntPtr(pbTmp);
            }

            if (cmdBufferTextBox.Text.StartsWith("0x"))
            {
                cmdTmp = long.Parse(cmdBufferTextBox.Text.Substring(2), System.Globalization.NumberStyles.HexNumber);
                AdressOffset.CMDBufferBase = new IntPtr(cmdTmp);
            }
            else
            {
                cmdTmp = long.Parse(cmdBufferTextBox.Text, System.Globalization.NumberStyles.HexNumber);
                AdressOffset.CMDBufferBase = new IntPtr(cmdTmp);
            }

            if (xpScaleTextBox.Text.StartsWith("0x"))
            {
                xpTmp = long.Parse(xpScaleTextBox.Text.Substring(2), System.Globalization.NumberStyles.HexNumber);
                AdressOffset.XPScaleBase = new IntPtr(xpTmp);
            }
            else
            {
                xpTmp = long.Parse(xpScaleTextBox.Text, System.Globalization.NumberStyles.HexNumber);
                AdressOffset.XPScaleBase = new IntPtr(xpTmp);
            }
            consoleOut("PlayerBase adress set : " + "0x" + AdressOffset.PlayerBase.ToInt64().ToString("X"));
            consoleOut("Cmdbuffer adress set : " + "0x" + AdressOffset.CMDBufferBase.ToInt64().ToString("X"));
            consoleOut("Xpscale adress set : " + "0x" + AdressOffset.XPScaleBase.ToInt64().ToString("X"));

            attachButton.Enabled = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                godmodCheck.Enabled = false;
                munInfCheck.Enabled = false;
                moneyInfCheck.Enabled = false;
                rapifFirecheck.Enabled = false;
                instaKillCheck.Enabled = false;
                moveSpeedTrackBar.Enabled = false;
                thermalScopeCheck.Enabled = false;
                tpZombiCheck.Enabled = false;
                changeWeaponButton.Enabled = false;
                godmodeAllCheck.Enabled = false;
                munitionInfAllCheck.Enabled = false;
                tpZombieSavePointCheck.Enabled = false;
                infMoneyAllCheck.Enabled = false;
                changeWPP2.Enabled = false;
                changeWPP3.Enabled = false;
                changeWPP4.Enabled = false;
                critKillCheck.Enabled = false;
                allCritKill.Enabled = false;
                autoFireCheck.Enabled = false;
                freeze0Check.Enabled = false;
                freeze1Check.Enabled = false;
                freeze2Check.Enabled = false;
                freeze3Check.Enabled = false;
                cmdBufferInput.Enabled = false;
                cmdBufferBtn.Enabled = false;
                kick2.Enabled = false;
                Kick3.Enabled = false;
                Kick4.Enabled = false;
                //activeXPCheck.Enabled = false;

                godmodCheck.Checked = false;
                munInfCheck.Checked = false;
                moneyInfCheck.Checked = false;
                rapifFirecheck.Checked = false;
                instaKillCheck.Checked = false;
                moveSpeedTrackBar.Value = 1;
                thermalScopeCheck.Checked = false;
                tpZombiCheck.Checked = false;
                godmodeAllCheck.Checked = false;
                munitionInfAllCheck.Checked = false;
                tpZombieSavePointCheck.Checked = false;
                infMoneyAllCheck.Checked = false;
                critKillCheck.Checked = false;
                allCritKill.Checked = false;
                autoFireCheck.Checked = false;
                freeze0Check.Checked = false;
                freeze1Check.Checked = false;
                freeze2Check.Checked = false;
                freeze3Check.Checked = false;
                //activeXPCheck.Checked = false;

                var gameProcs = Process.GetProcessesByName("BlackOpsColdWar");

                if (gameProcs.Length < 1) return;

                gameProc = gameProcs[0];

                gamePID = gameProc.Id;

                if (gamePID < 1) return;

                hProc = cwapi.OpenProcess(cwapi.ProcessAccessFlags.All, false, gameProc.Id);

                if (baseAddress != cwapi.GetModuleBaseAddress(gameProc, "BlackOpsColdWar.exe"))
                {
                    baseAddress = cwapi.GetModuleBaseAddress(gameProc, "BlackOpsColdWar.exe");
                }

                if (PlayerCompPtr != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64()), new int[] { 0 }))
                {
                    PlayerCompPtr = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64()), new int[] { 0 });
                }

                if (PlayerPedPtr != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64() + 0x8), new int[] { 0 }))
                {
                    PlayerPedPtr = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64() + 0x8), new int[] { 0 });
                }

                if (ZMGlobalBase != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64() + 0x60), new int[] { 0 }))
                {
                    ZMGlobalBase = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64() + 0x60), new int[] { 0 });
                }

                if (ZMBotBase != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64() + 0x68), new int[] { 0 }))
                {
                    ZMBotBase = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.PlayerBase.ToInt64()) + 0x68, new int[] { 0 });
                }

                if (ZMBotBase != (IntPtr)0x0 && ZMBotBase != (IntPtr)0x68 && ZMBotListBase != cwapi.FindDMAAddy(hProc, ZMBotBase + AdressOffset.ZM_Bot_List_Offset, new int[] { 0 }))
                {
                    ZMBotListBase = cwapi.FindDMAAddy(hProc, ZMBotBase + AdressOffset.ZM_Bot_List_Offset, new int[] { 0 });
                }

                for (int i = 0; i < 4; i++)
                {
                    cwapi.WriteProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * i) + AdressOffset.PC_GodMode, 0x20, 1, out _);
                }

                for (int i = 0; i < 4; i++)
                {
                    cwapi.WriteProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * i) + AdressOffset.CritKill8, 0, 1, out _);
                }
            }
            catch (Exception err)
            {
                consoleOut(err.Message);
            }
        }

        public double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public void RapidFire()
        {
            while (true)
            {
                if (rapifFirecheck.Checked && cwapi.GetAsyncKeyState(Keys.LButton) < 0)
                {
                    cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_RapidFire1, 0, 4, out _);
                    cwapi.WriteProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_RapidFire2, 0, 4, out _);
                    Thread.Sleep(10);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void InstaKill()
        {
            while (true)
            {
                Thread.Sleep(100);
                for (int i = 0; i < 90; i++)
                {
                    cwapi.WriteProcessMemory(hProc, (ZMBotListBase + AdressOffset.ZM_Bot_ArraySize_Offset * i) + AdressOffset.ZM_Bot_Health, 1, (long)4, out _);
                    byte[] tempHP = new byte[4];
                    cwapi.ReadProcessMemory(hProc, (ZMBotListBase + AdressOffset.ZM_Bot_ArraySize_Offset * i) + AdressOffset.ZM_Bot_MaxHealth, tempHP, 4, out _);
                    if (BitConverter.ToInt32(tempHP, 0) > 0)
                    {
                        cwapi.WriteProcessMemory(hProc, (ZMBotListBase + AdressOffset.ZM_Bot_ArraySize_Offset * i) + AdressOffset.ZM_Bot_MaxHealth, 1, (long)4, out _);
                    }
                }
            }
        }

        public void StatPlayerGrab()
        {
            while (true)
            {
                byte[] array = new byte[12];
                cwapi.ReadProcessMemory(this.hProc, this.PlayerPedPtr + AdressOffset.PP_Coords, array, 12L, out _);
                float num = BitConverter.ToSingle(array, 0);
                float num2 = BitConverter.ToSingle(array, 4);
                float num3 = BitConverter.ToSingle(array, 8);
                updatedPlayerPos = new Vector3((float)Math.Round((double)num, 4), (float)Math.Round((double)num2, 4), (float)Math.Round((double)num3, 4));
                UpdateLabel(posXLabel, updatedPlayerPos.X.ToString());
                UpdateLabel(posYLabel, updatedPlayerPos.Y.ToString());
                UpdateLabel(posZLabel, updatedPlayerPos.Z.ToString());

                for (int i = 0; i < 4; i++)
                {
                    byte[] _tempBuffer = new byte[100];
                    cwapi.ReadProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * i) + AdressOffset.PC_Name, _tempBuffer, 100, out _);
                    string a = System.Text.Encoding.UTF8.GetString(_tempBuffer);
                    switch (i)
                    {
                        case 0:
                            UpdateLabel(player1, a);
                            break;

                        case 1:
                            UpdateLabel(player2, a);
                            break;

                        case 2:
                            UpdateLabel(player3, a);
                            break;

                        case 3:
                            UpdateLabel(player4, a);
                            break;
                    }
                    Thread.Sleep(40);
                }
            }
        }

        public void CurrentWeapon()
        {
            while (true)
            {
                byte[] _tempBuffer = new byte[8];
                cwapi.ReadProcessMemory(hProc, PlayerCompPtr + AdressOffset.PC_CurrentUsedWeaponID, _tempBuffer, 8, out _);
                UpdateTextBox(currentWeaponText, BitConverter.ToInt64(_tempBuffer, 0).ToString());
                Thread.Sleep(200);
            }
        }

        public void TpZombie()
        {
            byte[] enemyPosBuffer = new byte[12];
            bool save = false;
            while (true)
            {
                if (tpZombiCheck.Checked && !tpZombieSavePointCheck.Checked)
                {
                    // gets current player position
                    byte[] playerHeadingXY = new byte[4];
                    byte[] playerHeadingZ = new byte[4];
                    cwapi.ReadProcessMemory(hProc, PlayerPedPtr + AdressOffset.PP_Heading_XY, playerHeadingXY, 4, out _);
                    cwapi.ReadProcessMemory(hProc, PlayerPedPtr + AdressOffset.PP_Heading_Z, playerHeadingZ, 4, out _);

                    // some stack overflow magic to get the direction the player is facing and getting a position in front of the player
                    double pitch = -ConvertToRadians(BitConverter.ToSingle(playerHeadingZ, 0));
                    double yaw = ConvertToRadians(BitConverter.ToSingle(playerHeadingXY, 0));
                    float x = Convert.ToSingle(Math.Cos(yaw) * Math.Cos(pitch));
                    float y = Convert.ToSingle(Math.Sin(yaw) * Math.Cos(pitch));
                    float z = Convert.ToSingle(Math.Sin(pitch));

                    // im guessing just a straight up BitConverter.GetBytes could have worked for writing vector3s to memory instead of this kinda messy solution
                    var newEnemyPos = updatedPlayerPos + new Vector3(x, y, z) * distanceTPBar.Value;

                    Buffer.BlockCopy(BitConverter.GetBytes(newEnemyPos.X), 0, enemyPosBuffer, 0, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(newEnemyPos.Y), 0, enemyPosBuffer, 4, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(newEnemyPos.Z), 0, enemyPosBuffer, 8, 4);

                    for (int i = 0; i < 90; i++)
                    {
                        cwapi.WriteProcessMemory(hProc, ZMBotListBase + (AdressOffset.ZM_Bot_ArraySize_Offset * i) + AdressOffset.ZM_Bot_Coords, enemyPosBuffer, 12, out _);
                    }
                }

                if (!tpZombiCheck.Checked && tpZombieSavePointCheck.Checked)
                {
                    if (!save)
                    {
                        byte[] playerHeadingXY = new byte[4];
                        byte[] playerHeadingZ = new byte[4];
                        cwapi.ReadProcessMemory(hProc, PlayerPedPtr + AdressOffset.PP_Heading_XY, playerHeadingXY, 4, out _);
                        cwapi.ReadProcessMemory(hProc, PlayerPedPtr + AdressOffset.PP_Heading_Z, playerHeadingZ, 4, out _);

                        double pitch = -ConvertToRadians(BitConverter.ToSingle(playerHeadingZ, 0));
                        double yaw = ConvertToRadians(BitConverter.ToSingle(playerHeadingXY, 0));
                        float x = Convert.ToSingle(Math.Cos(yaw) * Math.Cos(pitch));
                        float y = Convert.ToSingle(Math.Sin(yaw) * Math.Cos(pitch));
                        float z = Convert.ToSingle(Math.Sin(pitch));

                        zombieTpPos = updatedPlayerPos + new Vector3(x, y, z) * 150;

                        Buffer.BlockCopy(BitConverter.GetBytes(zombieTpPos.X), 0, enemyPosBuffer, 0, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(zombieTpPos.Y), 0, enemyPosBuffer, 4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(zombieTpPos.Z), 0, enemyPosBuffer, 8, 4);

                        if (cwapi.GetAsyncKeyState(Keys.RButton) < 0)
                        {
                            save = true;
                        }

                        for (int i = 0; i < 90; i++)
                        {
                            cwapi.WriteProcessMemory(hProc, ZMBotListBase + (AdressOffset.ZM_Bot_ArraySize_Offset * i) + AdressOffset.ZM_Bot_Coords, enemyPosBuffer, 12, out _);
                        }
                    }
                    else
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes(zombieTpPos.X), 0, enemyPosBuffer, 0, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(zombieTpPos.Y), 0, enemyPosBuffer, 4, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes(zombieTpPos.Z), 0, enemyPosBuffer, 8, 4);

                        for (int i = 0; i < 90; i++)
                        {
                            cwapi.WriteProcessMemory(hProc, ZMBotListBase + (AdressOffset.ZM_Bot_ArraySize_Offset * i) + AdressOffset.ZM_Bot_Coords, enemyPosBuffer, 12, out _);
                        }
                    }
                }

                if (!tpZombieSavePointCheck.Checked)
                {
                    save = false;
                }

                if (!tpZombiCheck.Checked && !tpZombieSavePointCheck.Checked)
                {
                    Thread.Sleep(500);
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }

        public void CmdBufferExec(string Command)
        {
            byte[] tempString = new byte[Command.Length];
            tempString = Encoding.UTF8.GetBytes(Command + "\0");
            cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.CMDBufferBase.ToInt64()), tempString, tempString.Length, out _);// Write Command
            cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.CMDBufferBase.ToInt64()) - 0x1B, (byte)1, 1, out _);// Execute
            Thread.Sleep(20);
            cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.CMDBufferBase.ToInt64()) - 0x1B, (byte)0, 1, out _);// Stop spam if Input-Command is wrong
            tempString = Encoding.UTF8.GetBytes("\0");
            cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + AdressOffset.CMDBufferBase.ToInt64()), tempString, tempString.Length, out _);// clear Input-Command
        }

        public void attachGame()
        {
            var gameProcs = Process.GetProcessesByName("BlackOpsColdWar");
            if (gameProcs.Length < 1)
            {
                consoleOut("GAME NOT RUNNING !");
                return;
            }
            gameProc = gameProcs[0];
            gamePID = gameProc.Id;
            if (gamePID < 1)
            {
                consoleOut("Game is not running !");
                return;
            }
            hProc = cwapi.OpenProcess(cwapi.ProcessAccessFlags.All, false, gameProc.Id);
            baseAddress = cwapi.GetModuleBaseAddress(gameProc, "BlackOpsColdWar.exe");
        }

        public void freezePlayer(object player)
        {
            byte[] array = new byte[12];
            cwapi.ReadProcessMemory(hProc, PlayerPedPtr + (AdressOffset.PP_ArraySize_Offset * (int)player) + AdressOffset.PP_Coords, array, 12L, out _);
            while (true)
            {
                cwapi.WriteProcessMemory(hProc, PlayerCompPtr + (AdressOffset.PC_ArraySize_Offset * (int)player) + AdressOffset.PC_Coords, array, 12L, out _);
                Thread.Sleep(60);
            }
        }
    }
}