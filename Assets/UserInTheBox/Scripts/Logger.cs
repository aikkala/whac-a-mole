using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UserInTheBox
{
    public class Logger
    {
        private Dictionary<string, StreamWriter> _files;
        private string _baseLogFolder;
        public bool Active { get; set; }


        public Logger()
        {
            _baseLogFolder = Path.Combine(Application.persistentDataPath, "logging/" + System.DateTime.Now.ToString("yyyy-MM-dd"));
            Debug.Log("Logs will be saved to " + _baseLogFolder);
            
            // Initialise stream holder
            _files = new Dictionary<string, StreamWriter>();
                
            // Create the output directory
            Directory.CreateDirectory(_baseLogFolder);
            
            // By default logger is not active
            Active = false;
        }

        public void Initialise(string key)
        {
            if (!Active)
            {
                return;
            }
            
            if (!_files.ContainsKey(key))
            {
                string logPath = Path.Combine(_baseLogFolder,
                    System.DateTime.Now.ToString("HH-mm-ss") + "-" + key + ".csv");
                _files.Add(key, new StreamWriter(logPath));
            }
            else
            {
                throw new IOException("A log file corresponding to key " + key + " has already been initialised");
            }
        }

        public void Finalise(string key)
        {
            if (!Active)
            {
                return;
            }
            
            if (_files.ContainsKey(key))
            {
                _files[key].Close();
                _files.Remove(key);
            }
        }

        // public async void Push(string key, string msg)
        public void Push(string key, string msg)
        {
            if (!Active)
            {
                return;
            }
            
            // Do we want async here? Does this even work in Unity?
            if (_files.ContainsKey(key))
            {
                // await _files[key].WriteLineAsync(msg);
                _files[key].WriteLine(msg);
            }
        }

        public void PushWithTimestamp(string key, string msg)
        {
            Push(key, Time.time + ", " + msg);
        }
    }
}
