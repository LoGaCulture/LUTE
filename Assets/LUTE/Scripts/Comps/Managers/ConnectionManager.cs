using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LoGaCulture.LUTE.Logs
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager Instance { get; private set; }
        private string serverAddress = "###";  // Fallback Server for now
        private string secretKey = string.Empty;

        private Queue<string> logQueue = new Queue<string>();  // Logs queue
        private bool isSendingLogs = false;
        private string logFilePath;

        private void Awake()
        {
            if (!LogaConstants.UseLogs)
            {
                //before final build, maybe change this line or maybe have a new method that just gets the secrets
                CheckForSecretsFile();
                return; 
            }
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scenes
                CheckForSecretsFile();
                logFilePath = Path.Combine(Application.persistentDataPath, "logs.json");

                // Load logs from file if available
                LoadLogsFromFile();

                DeleteLogFile();


            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            SaveLogsToFile();  // Save unsent logs when the application quits
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveLogsToFile();  // Save unsent logs when the application pauses
            }
        }

        private void CheckForSecretsFile()
        {
            string filePath = Path.Combine(Application.dataPath, "secrets.txt");
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line.StartsWith("ServerAddress="))
                    {
                        serverAddress = line.Replace("ServerAddress=", "").Trim();
                    }
                    else if (line.StartsWith("SecretKey="))
                    {
                        secretKey = line.Replace("SecretKey=", "").Trim();
                    }
                }
                Debug.Log($"Loaded ServerAddress: {serverAddress} and SecretKey from secrets.txt");
            }
            else
            {
                Debug.LogWarning("secrets.txt not found. Using default server address and no secret key.");
            }
        }

        public void EnqueueLog(UserLog log)
        {

            if (string.IsNullOrEmpty(secretKey))
            {
                Debug.LogWarning("Secret key not found. Logs will not be sent.");
                return;
            }

            string logJson = JsonUtility.ToJson(log);
            logQueue.Enqueue(logJson);  // Add log to queue

            if (!isSendingLogs)
            {
                StartCoroutine(SendLogs());
            }
        }

        // Send logs from queue to the server
        private IEnumerator SendLogs()
        {
            isSendingLogs = true;

            while (logQueue.Count > 0)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Debug.LogWarning("No internet connection for logs. Retrying...");
                    yield return new WaitForSeconds(5f);
                    continue;  // Retry until there is an internet connection
                }

                // Prepare log data for only one log entry (instead of the entire queue at once)
                string logData = logQueue.Peek();  // Get the first log without removing it

                string logJson = PrepareLogJson(new string[] { logData });

                UnityWebRequest request = new UnityWebRequest($"https://{serverAddress}/api/userlog", "POST");
                byte[] bodyRaw = Encoding.UTF8.GetBytes(logJson);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(secretKey))
                {
                    request.SetRequestHeader("X-Secret-Key", secretKey);
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Log successfully sent.");
                    logQueue.Dequeue();  // Remove the log from the queue after successful send

                    // Save any remaining logs in case we need to retry later
                    //SaveLogsToFile();
                }
                else
                {
                    Debug.LogError($"Failed to send log: {request.error}. Retrying...");
                    yield return new WaitForSeconds(5f);  // Retry after a delay
                }
            }

            isSendingLogs = false;
        }


        private void SaveLogsToFile()
        {
            if (!LogaConstants.UseLogs)
                return;
            if (logQueue.Count > 0)
            {
                // Wrap logs into a LogWrapper
                LogWrapper logWrapper = new LogWrapper
                {
                    logs = logQueue.Select(logJson => JsonUtility.FromJson<UserLog>(logJson)).ToArray()
                };

                // Serialize using JsonUtility
                string logsJson = JsonUtility.ToJson(logWrapper);
                File.WriteAllText(logFilePath, logsJson);
                Debug.Log($"Saved {logQueue.Count} logs to file at {logFilePath}");
            }
        }


        private void LoadLogsFromFile()
        {
            if (File.Exists(logFilePath))
            {
                string logsJson = File.ReadAllText(logFilePath);

                // Deserialize the entire LogWrapper object
                LogWrapper logWrapper = JsonUtility.FromJson<LogWrapper>(logsJson);

                foreach (var log in logWrapper.logs)
                {
                    logQueue.Enqueue(JsonUtility.ToJson(log));  // Restore logs to queue
                }

                Debug.Log($"Loaded {logWrapper.logs.Length} logs from file at {logFilePath}");
            }
        }



        // Delete the log file once logs are sent
        private void DeleteLogFile()
        {
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
                Debug.Log("Deleted log file after successful send");
            }
        }

        // Prepare logs for sending
        private string PrepareLogJson(string[] logs)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < logs.Length; i++)
            {
                sb.Append(logs[i]);
                if (i < logs.Length - 1) sb.Append(",");
            }
            sb.Append("]");
            return sb.ToString();
        }


        public void SaveSharedVariable(string variableName, string variableType, string data)
        {
            if (string.IsNullOrEmpty(secretKey))
            {
                Debug.LogWarning("Secret key not found. Shared variables will not be saved.");
                return;
            }


            Debug.Log("Saving shared variable of type " + variableType + " with name " + variableName + " and data " + data);

            SharedVariableDto sharedVariable = new SharedVariableDto
            {
                UUID = LogManager.Instance.UUID,
                VariableName = variableName,
                VariableType = variableType,
                Data = data,
                CreatedAt = DateTime.UtcNow.ToString("O")
            };
            string sharedVariableJson = JsonUtility.ToJson(sharedVariable);
            StartCoroutine(SendSharedVariable(sharedVariableJson));


        }

        //the api is at api/SharedVariable and wants a POST request with the shared variable data and the x-secret-key header
        private IEnumerator SendSharedVariable(string sharedVariableJson)
        {
            UnityWebRequest request = new UnityWebRequest($"https://{serverAddress}/api/SharedVariable", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(sharedVariableJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(secretKey))
            {
                request.SetRequestHeader("X-Secret-Key", secretKey);
            }
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Shared variable successfully saved.");
            }
            else
            {
                Debug.LogError($"Failed to save shared variable: {request.error}");
            }
        }


        public void FetchSharedVariables(string variableName, Action<SharedVariable[]> callback, int count = 1)
        {
            StartCoroutine(GetSharedVariables(variableName, callback, count));
        }

        private IEnumerator GetSharedVariables(string variableName, Action<SharedVariable[]> callback, int count)
        {
            string url = $"https://{serverAddress}/api/SharedVariable?variableName={variableName}&count={count}";
            UnityWebRequest request = UnityWebRequest.Get(url);

            request.SetRequestHeader("X-Secret-Key", secretKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Successfully retrieved shared variables: {request.downloadHandler.text}");

                // Deserialize JSON response into a list of SharedVariable objects
                List<SharedVariable> sharedVariables = JsonUtility.FromJson<SharedVariableList>("{\"variables\":" + request.downloadHandler.text + "}").variables;

                // Print each shared variable to the console
                foreach (var variable in sharedVariables)
                {
                    Debug.Log($"Variable: {variable.variableName}, Type: {variable.variableType}, Data: {variable.data}, CreatedAt: {variable.createdAt}");
                }

                callback?.Invoke(sharedVariables.ToArray());

            }
            else
            {
                Debug.LogError($"Failed to fetch shared variables: {request.error}");
                
                callback?.Invoke(null);

            }
        }


        [System.Serializable]
        public class LogWrapper
        {
            public UserLog[] logs;
        }


        //shared variable dto
        [System.Serializable]
        public class SharedVariableDto
        {
            public string UUID;
            public string VariableName;
            public string VariableType;
            public string Data;
            public string CreatedAt;
        }

        // DTO to match the API response
        [System.Serializable]
        public class SharedVariable
        {
            public string variableName;
            public string variableType;
            public string data;
            public string createdAt;
        }

        // Wrapper class to help deserialize a list of shared variables
        [System.Serializable]
        public class SharedVariableList
        {
            public List<SharedVariable> variables;
        }


    }
}
