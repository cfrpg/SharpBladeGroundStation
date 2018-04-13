using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Timers;

using Timer = System.Timers.Timer;

namespace SharpBladeGroundStation.CommunicationLink
{
    public class LogLink : CommLink
    {
        string path;
        string filename;
        FileStream stream;
        byte[] buffer;

        LogReplayState replayState;

       
        LinkPackage currentPackage;
        double currentPackageTime;
        LinkPackageDirection currPkgDir;
        DateTime openTime;
        bool timerRunning;

        Timer backgroundTimer;
        DateTime lastUpdateTime;

        double fullTime;
        double currentTime;
        double speed;

        List<Tuple<double, long>> logMetadata;

        public string Path
        {
            get { return path; }
        }

        public string Filename
        {
            get { return filename; }
        }

        public override string LinkName
        {
            get { return "Replay"; }
        }



        /// <summary>
        /// 回放的总时间，ms
        /// </summary>
        public double FullTime
        {
            get { return fullTime; }
            private set
            {
                fullTime = value;
                PropertyChangedEvent(this, "FullTime");
            }
        }

        /// <summary>
        /// 目前回放的时间进度，ms
        /// </summary>
        public double CurrentTime
        {
            get { return currentTime; }
            set
            {
                double t = value > fullTime ? fullTime : value;
                currentTime = Math.Round(t / 1000) * 1000;
                PropertyChangedEvent(this, "CurrentTime");
            }
        }

        /// <summary>
        /// 速度倍数
        /// </summary>
        public double Speed
        {
            get { return speed; }
            set
            {
                speed = value;
                PropertyChangedEvent(this, "Speed");
            }
        }

        public LogReplayState ReplayState
        {
            get { return replayState; }
        }



        /// <summary>
        /// 构造函数
        /// </summary>
        public LogLink() : base()
        {
            buffer = new byte[16384];           
            backgroundTimer = new Timer(20);
            backgroundTimer.Elapsed += BackgroundTimer_Elapsed;
            lastUpdateTime = DateTime.MinValue;
            replayState = LogReplayState.NoFile;
            timerRunning = false;
            speed = 1;
        }

        private void BackgroundTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!timerRunning)
                return;
            if (lastUpdateTime == DateTime.MinValue)
            {
                lastUpdateTime = e.SignalTime;
                return;
            }
            TimeSpan dt = e.SignalTime.Subtract(lastUpdateTime);
            lastUpdateTime = e.SignalTime;
            currentTime += dt.TotalMilliseconds * speed;
            bool res = getPackages();
            if (!res)
                replayState = LogReplayState.Stop;
            if (replayState == LogReplayState.Stop || replayState == LogReplayState.Pause)
            {
                timerRunning = false;
                backgroundTimer.Stop();
            }

        }

        private bool getPackages()
        {
            bool res = true;
            while(res)
            {               
                if (currentPackageTime < 0)
                {
                    res = readPackage();
                    continue;
                }
                if (currentTime >= currentPackageTime)
                {
                    if (currPkgDir == LinkPackageDirection.ToGCS)
                    {
                        receivePackage = currentPackage.Clone();
                        ReceivedPackageQueue.Enqueue(currentPackage.Clone());
                        OnReceivePackageEvent(this, new LinkEventArgs(receivePackage));
                    }
                    res = readPackage();
                }
                else
                    break;
            }
            return res;
        }

        public override void OpenLink()
        {
            if (replayState!=LogReplayState.NoFile)
            {                
                openTime = DateTime.Now;
                currentPackageTime = -1;
            }
        }

        public override void CloseLink()
        {
            if (replayState == LogReplayState.Stop)
            {
                replayState = LogReplayState.NoFile;
                stream.Close();
            }
        }

        public void Play()
        {
            if(replayState==LogReplayState.Pause)
            {
                replayState = LogReplayState.Playing;
                timerRunning = true;
                backgroundTimer.Start(); 
            }
        }

        public LoadFileResualt OpenFile(string p)
        {
            FileInfo fi = new FileInfo(p);
            if (!fi.Exists)
                return LoadFileResualt.NotExist;
            stream = new FileStream(p, FileMode.Open);
            stream.Read(buffer, 0, CommLogger.FileHeaderSize);
            byte[] header = { 83, 66, 76, 79, 71 };
            for (int i = 0; i < 5; i++)
            {
                if (buffer[i] != header[i])
                    return LoadFileResualt.UnkonwType;
            }

            protocol = (LinkProtocol)buffer[5];
            long time = BitConverter.ToInt64(buffer, 6);
            connectTime = DateTime.FromBinary(time);
            path = p;
            filename = fi.Name;
            switch (protocol)
            {
                case LinkProtocol.ANOLink:
                    currentPackage = new ANOLinkPackage();
                    break;
                case LinkProtocol.MAVLink:
                    currentPackage = new MAVLinkPackage();
                    break;
                case LinkProtocol.MAVLink2:
                    currentPackage = new MAVLinkPackage();
                    break;
                default:
                    currentPackage = new LinkPackage(2048);
                    break;
            }
            logMetadata = new List<Tuple<double, long>>();
            long pos;
            double ts;
            pos = stream.Position;
            while (readPackage())
            {
                ts = currentPackageTime;
                logMetadata.Add(new Tuple<double, long>(ts, pos));
                pos = stream.Position;
            }
            stream.Position = logMetadata[0].Item2;
            replayState = LogReplayState.Pause;
            currentPackageTime = -1;
            return LoadFileResualt.OK;
        }
             

        private bool readPackage()
        {
            long rem = stream.Length - stream.Position;
            if (rem < CommLogger.PackageHeaderSize)
                return false;
            stream.Read(buffer, 0, CommLogger.PackageHeaderSize);
            int len = BitConverter.ToInt32(buffer, 0);
            rem = stream.Length - stream.Position;
            if (rem < len)
                return false;
            currentPackageTime = BitConverter.ToDouble(buffer, 4);
            currPkgDir = (LinkPackageDirection)buffer[12];
            len -= CommLogger.PackageHeaderSize;
            stream.Read(buffer, 0, len);
            PackageParseResult res = currentPackage.ReadFromBuffer(buffer, len, 0);
            if (res != PackageParseResult.Yes)
            {

            }
            return true;
        }

        private int findPackage(double time)
        {
            int l = 0, r = logMetadata.Count - 1;
            int mid;
            while (l < r)
            {
                mid = (l + r) / 2;
                if (logMetadata[mid].Item1 < time)
                    l = mid + 1;
                else
                    r = mid;
            }
            return l;
        }
    }

}
