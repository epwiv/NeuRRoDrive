﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Microsoft.DirectX.DirectInput;
using System.Windows.Forms;

namespace CarPerformanceSimulator
{
    public partial class InputSettings : Form
    {
        string[] info;
        FormView mainDisplay;
        Device[] joystick;
        JoystickState[] state;
        DeviceCaps[] cps;
        bool axisNotFound = false;
        bool buttonNotFound = false;
        int axisToSet;
        int buttonToSet;
        bool controlInitalized = false;
        private enum autoDetectAxis { Steering = 0, Accelerator = 1, Brake = 2 };
        int joyToScan;
        int[] axis = new int[6]; //SHOULD BE STATIC STRUCT *ToDo
        Key[] pressedKeys;

        Label[] pollingReadouts;

        int[][] extraAxis;
        byte[][] buttons;


        string[] axisName = new string[6];


        public InputSettings(FormView display)
        {
            axisName[0] = "X";
            axisName[1] = "Y";
            axisName[2] = "Z";
            axisName[3] = "Rx";
            axisName[4] = "Ry";
            axisName[5] = "Rz";

            InitializeComponent();
            mainDisplay = display;
            pollingReadouts = new Label[2];
            pollingReadouts[0] = Joystick1Info;
            pollingReadouts[1] = Joystick2Info;
            InputSelect.SelectedIndex = mainDisplay.getInputSource();

            this.Show();
        }

        private void InputSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            mainDisplay.setInputSource(this.InputSelect.SelectedIndex);
            if (this.InputSelect.SelectedIndex == 0)
            {
                PollingTimer.Enabled = false;
                KeyboardOutput.Visible = false;
                WheelSelect.Items.Clear();
                PedalSelect.Items.Clear();
                joystick = mainDisplay.getJoystick();
                cps = new DeviceCaps[mainDisplay.getNumJoys()];
                for (int i = 0; i < mainDisplay.getNumJoys(); i++)
                {
                    string joy = "Joystick " + (i+1);
                    WheelSelect.Items.Add(joy);
                    PedalSelect.Items.Add(joy);
                    cps[i] = joystick[i].Caps;
                }
                state = mainDisplay.getJoystickState();
                SteeringAxisSelect.SelectedIndex = mainDisplay.getDrivingAxis();
                BrakeAxisSelect.SelectedIndex = mainDisplay.getBrakeAxis();
                AcceleratorAxisSelect.SelectedIndex = mainDisplay.getAccelAxis();
                ReverseButtonSelect.SelectedIndex = mainDisplay.getReverseButton();
                DriveButtonSelect.SelectedIndex = mainDisplay.getDriveButton();
                
                
                WheelSelect.SelectedIndex = mainDisplay.getPedalJoy();
                PedalSelect.SelectedIndex = mainDisplay.getWheelJoy();

                extraAxis = new int[mainDisplay.getNumJoys()][];
                buttons = new byte[mainDisplay.getNumJoys()][];
                info = new string[mainDisplay.getNumJoys()];

                controlInitalized = true;

                Joystick1Info.Visible = true;
                Joystick2Info.Visible = true;
                JoyPanel.Visible = true;
                PollingTimer.Enabled = true;
            }
            else
            {
                PollingTimer.Enabled = false;
                pressedKeys = mainDisplay.getPressedKeys();
                KeyboardOutput.Visible = true;
                Joystick1Info.Visible = false;
                Joystick2Info.Visible = false;
                JoyPanel.Visible = false;
                controlInitalized = false;
                PollingTimer.Enabled = true;
            }
        }

        private void pollingTimer_Tick(object sender, EventArgs e)
        {
            if (mainDisplay.getInputSource() == 1)
            {
                KeyboardOutput.Text = "Pressed Keys: ";
                pressedKeys = mainDisplay.getPressedKeys();
                if (pressedKeys != null && pressedKeys.Length > 0)
                {
                   foreach (Key key in pressedKeys)
                   {
                       KeyboardOutput.Text += key.ToString() + " ";
                   }
                }
            }
            else if (mainDisplay.getInputSource() == 0)
            {
                state = mainDisplay.getJoystickState();
                for (int i = 0; i < mainDisplay.getNumJoys(); i++)
                {
                    info[i] = "Joystick " + i + ": ";
                    //extraAxis[i] = state[i].GetSlider();
                    //info[i] += "A: " + extraAxis[i][0] + " ";
                    //info[i] += " B: " + extraAxis[i][1] + " ";
                    //Capture Position.
                    info[i] += " X:" + state[i].X + " ";
                    info[i] += " Y:" + state[i].Y + " ";
                    info[i] += " Z:" + state[i].Z + " ";

                    info[i] += " Rx:" + state[i].Rx + " ";
                    info[i] += " Ry:" + state[i].Ry + " ";
                    info[i] += " Rz:" + state[i].Rz + " ";
                    //display axis

                    // number of Axes
                    info[i] += " Joystick Axis: " + cps[i].NumberAxes;
                    // number of Buttons
                    info[i] += " Joystick Buttons: " + cps[i].NumberButtons;

                    info[i] += " Joystick ID: " + joystick[i].Properties.JoystickId;

                    //info[i] += " Joystick AxisMAX: " + joystick[i].Properties.GetRange(ParameterHow.ByOffset, joystick[i].Properties.JoystickId).Max;

                    //Capture Buttons.
                    buttons[i] = state[i].GetButtons();
                    for (int j = 0; j < buttons[i].Length; j++)
                    {
                        if (buttons[i][j] != 0)
                        {
                            info[i] += "Button:" + j + " ";
                        }
                    }
                    //display joystick settings
                    pollingReadouts[i].Text = info[i];
                }



                if (axisNotFound)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (axis[i] != (int)state[joyToScan].GetType().GetProperty(axisName[i]).GetValue(state[joyToScan], null))
                        {
                            switch (axisToSet)
                            {
                                case 0:
                                    SteeringAxisSelect.SelectedIndex = i;
                                    break;
                                case 1:
                                    AcceleratorAxisSelect.SelectedIndex = i;
                                    break;
                                case 2:
                                    BrakeAxisSelect.SelectedIndex = i;
                                    break;
                            }

                            axisNotFound = false;
                        }
                    }
                }
                if (buttonNotFound)
                {
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        if (buttons[joyToScan][i] != 0)
                        {
                            switch (buttonToSet)
                            {
                                case 0:
                                    DriveButtonSelect.SelectedIndex = i;
                                    break;
                                case 1:
                                    ReverseButtonSelect.SelectedIndex = i;
                                    break;
                            }
                            buttonNotFound = false;
                        }
                    }
                }
            }
        }

        private void autoDetect()
        {
            axis[0] = state[joyToScan].X;
            axis[1] = state[joyToScan].Y;
            axis[2] = state[joyToScan].Z;
            axis[3] = state[joyToScan].Rx;
            axis[4] = state[joyToScan].Ry;
            axis[5] = state[joyToScan].Rz;

            axisNotFound = true; //ADD TIMEOUT *ToDo
        }
        
        private void AutoSteering_Click(object sender, EventArgs e)
        {
            axisToSet = (int)autoDetectAxis.Steering;
            joyToScan = WheelSelect.SelectedIndex;
            autoDetect();
        }
        
        private void AutoAccelerator_Click(object sender, EventArgs e)
        {
            axisToSet = (int)autoDetectAxis.Accelerator;
            joyToScan = PedalSelect.SelectedIndex;
            autoDetect();
        }

        private void AutoBrake_Click(object sender, EventArgs e)
        {
            axisToSet = (int)autoDetectAxis.Brake;
            joyToScan = PedalSelect.SelectedIndex;
            autoDetect();
        }

        private void AcceleratorAxisSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(controlInitalized)
            mainDisplay.setAccelAxis(axisName[AcceleratorAxisSelect.SelectedIndex]);
        }

        private void BrakeAxisSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (controlInitalized)
            mainDisplay.setBrakeAxis(axisName[BrakeAxisSelect.SelectedIndex]);
        }

        private void SteeringAxisSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (controlInitalized)
            mainDisplay.setDrivingAxis(axisName[SteeringAxisSelect.SelectedIndex]);
        }

        private void DriveButtonSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (controlInitalized)
            mainDisplay.setDriveButton(DriveButtonSelect.SelectedIndex);
        }

        private void ReverseButtonSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (controlInitalized)
            mainDisplay.setReverseButton(ReverseButtonSelect.SelectedIndex);
        }

        private void AutoDrive_Click(object sender, EventArgs e)
        {
            buttonToSet = 0;
            joyToScan = WheelSelect.SelectedIndex;
            buttonNotFound = true;
        }

        private void AutoReverse_Click(object sender, EventArgs e)
        {
            buttonToSet = 1;
            joyToScan = WheelSelect.SelectedIndex;
            buttonNotFound = true;
        }

        private void WheelSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(controlInitalized)
            mainDisplay.setWheelJoy(WheelSelect.SelectedIndex);
        }

        private void PedalSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (controlInitalized)
                mainDisplay.setPedalJoy(PedalSelect.SelectedIndex);
        }

        
    }
}