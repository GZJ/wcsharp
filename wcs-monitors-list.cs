using System;
using System.Windows.Forms;

class Program
{
    static void Main()
    {
        Application.EnableVisualStyles();

        int index = 0;
        foreach (Screen screen in Screen.AllScreens)
        {
            Console.Write(
                $"Monitor {index}:\n"
                    + $"Device Name: {screen.DeviceName}\n"
                    + $"Bounds: {screen.Bounds}\n"
                    + $"Working Area: {screen.WorkingArea}\n"
                    + $"Primary: {screen.Primary}\n"
            );
            Console.WriteLine();
            index++;
        }
    }
}
