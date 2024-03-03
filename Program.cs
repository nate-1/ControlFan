// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

namespace ControlFan; 

class Program 
{
    static readonly string SENSOR = "/sys/class/hwmon/hwmon2/temp2_input";
    static readonly int MIN_CPU_TEMP = 45000;
    static readonly int MAX_CPU_TEMP = 67000;
    static readonly int CRITCAL_TEMP = 80000;
    static readonly int INTEVAL = 10000;

    static string currentCPUFanState = "";
    static string currentMotherBoardFanState = "";
    static void Main()
    {
        while(true)
        {
            ControlFan(ReadValue(SENSOR));
            Thread.Sleep(INTEVAL);
        }
    }

    

    private static void ControlFan(int temp)
    {
        Console.WriteLine(temp);
        Console.WriteLine(currentMotherBoardFanState + " " + currentCPUFanState);
        if (temp >= CRITCAL_TEMP)
        {
            if(currentMotherBoardFanState != "2" || currentCPUFanState != "2")
                SetFanSpeed("2", "2");
        }
        else if(temp >= MAX_CPU_TEMP)
        {
            if(currentMotherBoardFanState != "1" || currentCPUFanState != "2")
                SetFanSpeed("1", "2");
        }
        else if(temp >= MIN_CPU_TEMP)
        {
            if(currentMotherBoardFanState != "0" || currentCPUFanState != "1")
                SetFanSpeed("0", "1");
        }
        else 
        {
            if(currentMotherBoardFanState != "0" || currentCPUFanState != "0")
                SetFanSpeed("0", "0");
        }
    }

    private static void SetFanSpeed(string motherboard, string cpu)
    {
        Process p = new Process();
        p.StartInfo.FileName = "/usr/bin/i8kfan"; 
        p.StartInfo.Arguments = $"{motherboard} {cpu}";
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        p.WaitForExit();

        Console.WriteLine($"Changing fan speed: mb={motherboard} cpu={cpu}");

        currentMotherBoardFanState = motherboard; 
        currentCPUFanState = cpu;
    }

    private static int ReadValue(string path)
    {
        string? line;
        using(StreamReader sr = new StreamReader(path))
        {
            line = sr.ReadLine();
            sr.Close();
        }

        if(int.TryParse(line, out int res))
            return res; 
        else
        {
            Console.WriteLine("Error could not read: " + path);
            return 0;
        }
    }
}