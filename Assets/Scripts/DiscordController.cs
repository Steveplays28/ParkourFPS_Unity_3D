using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscordController : MonoBehaviour
{
    public static DiscordController instance;

    private Discord.Discord discord;
    private Discord.ActivityManager activityManager;

    [Header("Rich Presence")]
    public string state;
    public string details;

    public string largeImage;
    public string largeImageTooltip;
    public string smallImage;
    public string smallImageTooltip;

    public long startTime;
    public long endTime;

    public int currentPartySize;
    public int maxPartySize;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        try
        {
            discord = new Discord.Discord(760578869500575816, (int)Discord.CreateFlags.NoRequireDiscord);
            discord.SetLogHook(Discord.LogLevel.Debug, LogProblemsFunction);
            activityManager = discord.GetActivityManager();
            startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            UpdateActivity();

            activityManager.OnActivityJoin += secret =>
            {
                Debug.Log("join + " + secret);
                Client.instance.ConnectToServer(secret.Split(':')[0], int.Parse(secret.Split(':')[1]));
            };
            //activityManager.OnActivityJoinRequest += (ref Discord.User user) =>
            //{
            //    Debug.Log("joinrequest");
            //    activityManager.SendInvite(user.Id, Discord.ActivityActionType.Join, "Come play!", (result) =>
            //    {
            //        Debug.Log(result);
            //    });
            //};
        }
        catch (Exception discordException)
        {
            Debug.LogError(discordException);
        }

        AsyncOperation sceneLoadAsyncOperation;
        sceneLoadAsyncOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        sceneLoadAsyncOperation.completed += OnMainMenuLoaded;
    }

    private void OnMainMenuLoaded(AsyncOperation asyncOperation)
    {
        if (discord == null)
        {
            return;
        }

        state = "In main menu";
        details = "";
        largeImageTooltip = "";
        startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        UpdateActivity();
    }

    private void LogProblemsFunction(Discord.LogLevel level, string message)
    {
        Debug.Log($"Discord:{level} - {message}");
    }

    private void OnApplicationQuit()
    {
        if (discord == null)
        {
            return;
        }

        try
        {
            discord.Dispose();
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
        }
    }

    private void Update()
    {
        if (discord == null)
        {
            return;
        }

        try
        {
            discord.RunCallbacks();
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
        }
    }

    public void UpdateActivity()
    {
        if (discord == null)
        {
            return;
        }

        try
        {
            Discord.Activity activity = new Discord.Activity
            {
                State = state,
                Details = details,
                Assets =
                {
                    LargeImage = largeImage,
                    LargeText = largeImageTooltip,
                    SmallImage = smallImage,
                    SmallText = smallImageTooltip
                },
                Timestamps =
                {
                    Start = startTime
                }
            };

            if (Client.instance != null && Client.instance.isConnected)
            {
                activity.Party = new Discord.ActivityParty
                {
                    Id = GenerateRandomString(7),
                    Size =
                    {
                        CurrentSize = currentPartySize,
                        MaxSize = maxPartySize
                    },
                };
                activity.Secrets = new Discord.ActivitySecrets
                {
                    Join = string.Concat(Client.instance.ip, ":", Client.instance.port)
                };
            }

            activityManager.UpdateActivity(activity, (result) =>
            {
                if (result == Discord.Result.Ok)
                {
                    Debug.Log("Successfully set Discord RPC!");
                }
                else
                {
                    Debug.LogError("Error setting Discord RPC: " + result.ToString());
                }
            });
        }
        catch (Exception exception)
        {
            Debug.LogError(exception);
        }
    }

    private string GenerateRandomString(int length)
    {
        // Creating a StringBuilder object
        StringBuilder str_build = new StringBuilder();
        System.Random random = new System.Random();

        // Generating a random string
        string letter = "";
        for (int i = 0; i < length; i++)
        {
            double flt = random.NextDouble();
            int shift = Convert.ToInt32(Math.Floor(25 * flt));
            letter = Convert.ToString(shift + 65);
            str_build.Append(letter);
        }

        return letter;
    }
}
