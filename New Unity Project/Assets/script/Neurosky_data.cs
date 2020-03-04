using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//using System.IO.Ports;
using UnityEngine.UI;
using System.Threading;
using System.Net.Sockets;
using Jayrock.Json.Conversion;
using System;
using System.Text;

public class Neurosky_data : MonoBehaviour
{
    private bool ready = false;
    private int check = 1;
    private StringBuilder csv;
    public int attention = 0;
    IDictionary eegPower;
    IDictionary eSense;
    TcpClient client;
    Stream stream;
    byte[] buffer = new byte[4096];
    int bytesRead; // Building command to enable JSON output from ThinkGear Connector (TGC) 
    public seesaaa seesaw_code;
    public int baseline_check = 0;
    public int Average_bl = 0;
    
    // Start is called before the first frame update
    void Start()
    {
                
        var com = @"{""enableRawOutput"": false, ""format"": ""Json""}";
        byte[] myWriteBuffer = Encoding.ASCII.GetBytes(com);

        try
        {
            Debug.Log("Starting connection to Mindwave Mobile Headset.");
            client = new TcpClient("127.0.0.1", 13854);
            stream = client.GetStream();
            System.Threading.Thread.Sleep(500);
            client.Close();
            Debug.Log("Step 1 completed!!!");
        }
        catch (SocketException se)
        {
            Debug.Log("Error connecting to device.");
        }


        try
        {
            client = new TcpClient("127.0.0.1", 13854);
            stream = client.GetStream();

            Debug.Log("Sending configuration packet to device.");
            if (stream.CanWrite)
                stream.Write(myWriteBuffer, 0, myWriteBuffer.Length);

            System.Threading.Thread.Sleep(500);
            client.Close();

            Debug.Log("Step 2 completed!!!");
        }
        catch (SocketException se)
        {
            Debug.Log("Error sending configuration packet to TGC.");
        }


        try
        {
            Debug.Log("Starting data collection.");
            client = new TcpClient("127.0.0.1", 13854);
            stream = client.GetStream();

            // Sending configuration packet to TGC                
            if (stream.CanWrite)
                stream.Write(myWriteBuffer, 0, myWriteBuffer.Length);


            if (stream.CanRead)
            {
                //to check if device is ready
                ready = false;
                //to note keyboard key press and note key press
                Debug.Log("Enter any key to start.");
                ConsoleKeyInfo key = Console.ReadKey(false);
                Debug.Log("Reading bytes");

                System.Threading.Thread.Sleep(500);
                //client.Close();
            }
        }
        catch (SocketException se)
        {
            Debug.Log("Error in data collection.");
        }


    }


    // Update is called once per frame
    void Update()
    {
        if (check == 1)
        {
            csv = new StringBuilder();
            csv.AppendLine("attention,meditation,delta,theta,lowAplha,highAlpha,lowBeta,highBeta,lowGamma,highGamma");
            check += 1;
        }
        bytesRead = stream.Read(buffer, 0, 4096);
        //Console.WriteLine("1st step after Reading bytes");
        // This should really be in it's own thread  
        string[] packets = Encoding.UTF8.GetString(buffer, 0, bytesRead).Split('\r');
        foreach (string s in packets)
        {
            try
            {
                IDictionary data = (IDictionary)JsonConvert.Import(s);
                //Check if device is ON/OFF
                if (data.Contains("status"))
                {
                    Debug.Log("Device is Off.");
                    ready = false;
                    break;
                }

                //Check fitting (device on head or not)
                if (data.Contains("eSense"))
                    Debug.Log("esense steps");
                if (data["eSense"].ToString() == "{\"attention\":0,\"meditation\":0}")
                {
                    Debug.Log("Check fitting.");
                    ready = false;
                    break;
                }

                //check if device is ready
                if ((data.Contains("eSense")) && (ready == false))
                {
                    IDictionary d = (IDictionary)data["eSense"];
                    if ((d["attention"].ToString() != "0") && (d["meditation"].ToString() != "0"))
                    {
                        ready = true;
                        Debug.Log("Device is ready.");
                    }
                }

                //start data reading only when device is ready.
                baseline_check = seesaw_code.baseline_timer;
                name = seesaw_code.theName;

                if (ready == true && baseline_check == 1)
                {
                    Debug.Log("Data Available");
                    eSense = (IDictionary)data["eSense"];
                    eegPower = (IDictionary)data["eegPower"];

                    csv.AppendLine(eSense["attention"].ToString() + "," + eSense["meditation"].ToString() + "," + eegPower["" +
                        "delta"].ToString() + "," + eegPower["theta"].ToString() + "," + eegPower["lowAlpha"].ToString() + "," + eegPower["highAlpha"].ToString() + ","
                        + eegPower["lowBeta"].ToString() + "," + eegPower["highBeta"].ToString() + "," + eegPower["lowGamma"].ToString() + "," +
                        eegPower["highGamma"].ToString() + "\n");

                    File.WriteAllText(@"D:\" + name + "baseline" + ".csv", csv.ToString());
                    Debug.Log(eSense["attention"].GetType());
                    attention = Convert.ToInt32(eSense["attention"]);
                    //bl_attention.Add(attention);
                    Debug.Log(attention);

                    int count = 0;
                    int sum = 0;
                    count++;
                    sum += attention;
                    Average_bl = sum / count;
                }
                else if (ready == true && baseline_check == 2)
                {
                    Debug.Log("Data Available");
                    eSense = (IDictionary)data["eSense"];
                    eegPower = (IDictionary)data["eegPower"];

                    csv.AppendLine(eSense["attention"].ToString() + "," + eSense["meditation"].ToString() + "," + eegPower["" +
                        "delta"].ToString() + "," + eegPower["theta"].ToString() + "," + eegPower["lowAlpha"].ToString() + "," + eegPower["highAlpha"].ToString() + ","
                        + eegPower["lowBeta"].ToString() + "," + eegPower["highBeta"].ToString() + "," + eegPower["lowGamma"].ToString() + "," +
                        eegPower["highGamma"].ToString() + "\n");

                    File.WriteAllText(@"D:\" + name + "real_time" + ".csv", csv.ToString());
                    Debug.Log(eSense["attention"].GetType());
                    attention = Convert.ToInt32(eSense["attention"]);
                    Debug.Log(attention);
                }
            }

            catch (Exception e)
            {
            }
        }

    }
}
