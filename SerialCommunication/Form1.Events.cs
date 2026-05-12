using System;
using System.Windows.Forms;

namespace SerialCommunication
{
    public partial class Form1
    {
        private void trackBarPWM9_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    string commando = $"set pwm9 {trackBarPWM9.Value}";
                    serialPortArduino.WriteLine(commando);
                }
                else
                {
                    MessageBox.Show("Serial port is not open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij het versturen: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBoxDigital2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    string commando = checkBoxDigital2.Checked ? "set d2 high" : "set d2 low";
                    serialPortArduino.WriteLine(commando);
                }
                else
                {
                    MessageBox.Show("Serial port is not open.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fout bij het versturen: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            timerOefening3.Enabled = (tabControl.SelectedTab == tabPageOefening3);
            timerOefening4.Enabled = (tabControl.SelectedTab == tabPageOefening4);
            timerOefening5.Enabled = (tabControl.SelectedTab == tabPageOefening5);
        }

        private void timerOefening3_Tick(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    // Clear any previous data
                    serialPortArduino.ReadExisting();

                    // Digital 5
                    serialPortArduino.WriteLine("digital? 5");
                    string a5 = serialPortArduino.ReadLine();
                    a5 = a5.Trim();
                    radioButtonDigital5.Checked = (a5 == "1");

                    // Digital 6
                    serialPortArduino.WriteLine("digital? 6");
                    string a6 = serialPortArduino.ReadLine();
                    a6 = a6.Trim();
                    radioButtonDigital6.Checked = (a6 == "1");

                    // Digital 7
                    serialPortArduino.WriteLine("digital? 7");
                    string a7 = serialPortArduino.ReadLine();
                    a7 = a7.Trim();
                    radioButtonDigital7.Checked = (a7 == "1");
                }
            }
            catch (Exception ex)
            {
                // Simple error reporting
                try { labelStatus.Text = "error: " + ex.Message; } catch { }
            }
        }

        private void timerOefening4_Tick(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    // Clear any previous data
                    serialPortArduino.ReadExisting();

                    // Request analog 0
                    serialPortArduino.WriteLine("get a0");
                    string antwoord = serialPortArduino.ReadLine();
                    antwoord = antwoord.Trim();

                    // Extract digits only (value)
                    string value = "";
                    foreach (char c in antwoord)
                    {
                        if (char.IsDigit(c))
                            value += c;
                    }

                    if (labelAnalog0.InvokeRequired)
                    {
                        labelAnalog0.BeginInvoke((Action)(() => labelAnalog0.Text = value));
                    }
                    else
                    {
                        labelAnalog0.Text = value;
                    }
                }
            }
            catch (Exception ex)
            {
                try { labelStatus.Text = "error: " + ex.Message; } catch { }
            }
        }

        private void timerOefening5_Tick(object sender, EventArgs e)
        {
            try
            {
                if (serialPortArduino != null && serialPortArduino.IsOpen)
                {
                    // Clear any previous data
                    serialPortArduino.ReadExisting();

                    // Read desired temperature from A0
                    serialPortArduino.WriteLine("get a0");
                    string antwoord0 = serialPortArduino.ReadLine();
                    antwoord0 = antwoord0.Trim();
                    string value0 = "";
                    foreach (char c in antwoord0) { if (char.IsDigit(c)) value0 += c; }
                    int adc0 = 0; int.TryParse(value0, out adc0);
                    double m0 = 40.0 / 1023.0; double desired = m0 * adc0 + 5.0;

                    // Read current temperature from A1
                    serialPortArduino.WriteLine("get a1");
                    string antwoord1 = serialPortArduino.ReadLine();
                    antwoord1 = antwoord1.Trim();
                    string value1 = "";
                    foreach (char c in antwoord1) { if (char.IsDigit(c)) value1 += c; }
                    int adc1 = 0; int.TryParse(value1, out adc1);
                    double m1 = 500.0 / 1023.0; double current = m1 * adc1;

                    string desiredText = desired.ToString("0.0") + " °C";
                    string currentText = current.ToString("0.0") + " °C";

                    if (labelGewensteTemp.InvokeRequired)
                    {
                        labelGewensteTemp.BeginInvoke((Action)(() => labelGewensteTemp.Text = desiredText));
                    }
                    else { labelGewensteTemp.Text = desiredText; }

                    if (labelHuidigeTemp.InvokeRequired)
                    {
                        labelHuidigeTemp.BeginInvoke((Action)(() => labelHuidigeTemp.Text = currentText));
                    }
                    else { labelHuidigeTemp.Text = currentText; }

                    // LED control on digital pin 2: turn on when current < desired
                    string cmd = (current < desired) ? "set d2 high" : "set d2 low";
                    serialPortArduino.WriteLine(cmd);
                }
                else
                {
                    try { labelStatus.Text = "status : No serial connection"; } catch { }
                }
            }
            catch (Exception ex)
            {
                try { labelStatus.Text = "error: " + ex.Message; } catch { }
            }
        }

    }
}
