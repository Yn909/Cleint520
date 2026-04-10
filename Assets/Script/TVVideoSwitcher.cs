using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(VideoPlayer))]
public class TVVideoSwitcher : MonoBehaviour
{
    [Header("引用")]
    public Camera playerCamera;
    public VideoPlayer videoPlayer;
    public MeshRenderer screenRenderer;

    [Header("视频列表")]
    public VideoClip[] videoClips;

    [Header("交互参数")]
    public float interactDistance = 5f;
    public LayerMask interactLayer;

    [Header("按键设置")]
    public KeyCode playOrStopKey = KeyCode.T;
    public KeyCode nextVideoKey = KeyCode.N;

    private int currentVideoIndex = 0;
    private bool isPlaying = false;
    private bool isPreparing = false;

    private RenderTexture renderTexture; // ⭐ 每个TV独立RT
    private Ray ray;

    public TVDevice vDevice;

    void Start()
    {
        playerCamera = Camera.main;
        vDevice = GetComponent<TVDevice>();
        // 获取组件
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        //  材质实例化（避免共用材质）
        if (screenRenderer != null)
        {
            screenRenderer.material = new Material(screenRenderer.material);
        }

        //  创建独立 RenderTexture
        renderTexture = new RenderTexture(1920, 1080, 0);
        renderTexture.Create();

        // 绑定 VideoPlayer
        videoPlayer.targetTexture = renderTexture;

        // 绑定材质贴图
        if (screenRenderer != null)
        {
            screenRenderer.material.mainTexture = renderTexture;
        }

        // 初始黑屏
        ShowOffScreen();

        // VideoPlayer 设置
        videoPlayer.playOnAwake = false;
        videoPlayer.Stop();

        videoPlayer.prepareCompleted += OnPrepareCompleted;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void Update()
    {
        if (Input.GetKeyDown(playOrStopKey))
        {
            if (CanInteractWithTV())
            {
                TogglePlayOrStop();
            }
        }

        if (Input.GetKeyDown(nextVideoKey))
        {
            if (CanInteractWithTV())
            {
                PlayNextVideo();
            }
        }
    }

    bool CanInteractWithTV()
    {
        if (playerCamera == null)
            return false;

        ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                return true;
            }
        }

        return false;
    }

    public void TogglePlayOrStop()
    {
        if (videoClips == null || videoClips.Length == 0)
        {
            Debug.LogWarning("没有设置视频列表");
            return;
        }

        if (isPlaying || isPreparing)
        {
            StopVideo();
            vDevice.isOn = false;
        }
        else
        {
            vDevice.isOn = true;
            PlayCurrentVideo();
        }
        WebSocketManager.Instance.SendDeviceUpdate(vDevice);
    }

    public void PlayCurrentVideo()
    {
        if (videoClips == null || videoClips.Length == 0) return;

        currentVideoIndex = Mathf.Clamp(currentVideoIndex, 0, videoClips.Length - 1);

        videoPlayer.Stop();
        videoPlayer.clip = videoClips[currentVideoIndex];

        ShowPlayingScreen();

        isPreparing = true;
        isPlaying = false;

        videoPlayer.Prepare();
    }

    public void PlayNextVideo()
    {
        if (videoClips == null || videoClips.Length == 0) return;

        currentVideoIndex++;
        if (currentVideoIndex >= videoClips.Length)
        {
            currentVideoIndex = 0;
        }
        vDevice.channel = currentVideoIndex+1;
        WebSocketManager.Instance.SendDeviceUpdate(vDevice);
        PlayCurrentVideo();
    }

    public void PlayNextVideo(int index)
    {
        if (videoClips == null || videoClips.Length == 0) return;

        currentVideoIndex = index - 1;
        if (currentVideoIndex >= videoClips.Length)
        {
            currentVideoIndex = 0;
        }

        PlayCurrentVideo();
    }

    public void StopVideo()
    {
        videoPlayer.Stop();

        isPlaying = false;
        isPreparing = false;

        ShowOffScreen();
    }

    void OnPrepareCompleted(VideoPlayer source)
    {
        isPreparing = false;
        isPlaying = true;
        source.Play();
    }

    void OnVideoFinished(VideoPlayer source)
    {
        isPlaying = false;
        isPreparing = false;
        ShowOffScreen();
    }

    void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError("视频播放错误: " + message);
        isPlaying = false;
        isPreparing = false;
        ShowOffScreen();
    }

    //  不再切换材质，只改贴图
    void ShowOffScreen()
    {
        if (screenRenderer != null)
        {
            screenRenderer.material.mainTexture = null;
            screenRenderer.material.color = Color.black;
        }
    }

    void ShowPlayingScreen()
    {
        if (screenRenderer != null)
        {
            screenRenderer.material.mainTexture = renderTexture;
            screenRenderer.material.color = Color.white;
        }
    }

    //  防止内存泄漏（非常重要）
    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}