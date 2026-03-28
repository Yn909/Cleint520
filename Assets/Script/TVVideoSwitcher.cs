using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(Collider))]
public class TVVideoSwitcher : MonoBehaviour
{
    [Header("引用")]
    public Camera playerCamera;                 // 玩家摄像机
    public VideoPlayer videoPlayer;             // 视频播放器
    public MeshRenderer screenRenderer;         // 电视屏幕的 MeshRenderer

    [Header("材质")]
    public Material offMaterial;                // 关闭时黑屏材质
    public Material playingMaterial;            // 播放时材质（挂了 RenderTexture 的材质）

    [Header("视频列表")]
    public VideoClip[] videoClips;              // 你的3个视频

    [Header("交互参数")]
    public float interactDistance = 5f;         // 可交互距离
    public LayerMask interactLayer;             // 电视所在层

    [Header("按键设置")]
    public KeyCode playOrStopKey = KeyCode.T;   // 播放 / 暂停（关闭）
    public KeyCode nextVideoKey = KeyCode.N;    // 下一个视频

    private int currentVideoIndex = 0;
    private bool isPlaying = false;
    private bool isPreparing = false;
    Ray ray; 

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // 初始黑屏
        ShowOffScreen();

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.Stop();

            videoPlayer.prepareCompleted += OnPrepareCompleted;
            videoPlayer.errorReceived += OnVideoError;
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void Update()
    {
        // F：播放 / 关闭
        if (Input.GetKeyDown(playOrStopKey))
        {
            if (CanInteractWithTV())
            {
                TogglePlayOrStop();
            }
        }

        // R：下一个视频
        if (Input.GetKeyDown(nextVideoKey))
        {
            if (CanInteractWithTV())
            {
                PlayNextVideo();
            }
        }
    }

    /// <summary>
    /// 是否允许和电视交互
    /// 必须屏幕中心准星对准电视，并且在交互距离内
    /// </summary>
    bool CanInteractWithTV()
    {
        Debug.Log("aaa,,,,");
        if (playerCamera == null)
            return false;

        ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer, QueryTriggerInteraction.Ignore))
        {
            // 命中自己或自己的子物体
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// F 键逻辑：播放 / 关闭
    /// </summary>
    public void TogglePlayOrStop()
    {
        if (videoClips == null || videoClips.Length == 0)
        {
            Debug.LogWarning("没有设置视频列表");
            return;
        }

        if (videoPlayer == null)
        {
            Debug.LogWarning("没有设置 VideoPlayer");
            return;
        }

        if (isPlaying || isPreparing)
        {
            StopVideo();
        }
        else
        {
            PlayCurrentVideo();
        }
    }

    /// <summary>
    /// 播放当前视频
    /// </summary>
    public void PlayCurrentVideo()
    {
        if (videoClips == null || videoClips.Length == 0) return;
        if (videoPlayer == null) return;

        currentVideoIndex = Mathf.Clamp(currentVideoIndex, 0, videoClips.Length - 1);

        videoPlayer.Stop();
        videoPlayer.clip = videoClips[currentVideoIndex];

        // 切到播放材质
        ShowPlayingScreen();

        isPreparing = true;
        isPlaying = false;

        videoPlayer.Prepare();
    }

    /// <summary>
    /// R 键逻辑：切换下一个视频并播放
    /// </summary>
    public void PlayNextVideo()
    {
        if (videoClips == null || videoClips.Length == 0) return;
        if (videoPlayer == null) return;

        currentVideoIndex++;
        if (currentVideoIndex >= videoClips.Length)
        {
            currentVideoIndex = 0;
        }

        PlayCurrentVideo();
    }

    /// <summary>
    /// 停止播放并黑屏
    /// </summary>
    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        isPlaying = false;
        isPreparing = false;

        ShowOffScreen();
    }

    /// <summary>
    /// 视频准备完成后自动播放
    /// </summary>
    void OnPrepareCompleted(VideoPlayer source)
    {
        isPreparing = false;
        isPlaying = true;
        source.Play();
    }

    /// <summary>
    /// 视频播放结束后恢复黑屏
    /// </summary>
    void OnVideoFinished(VideoPlayer source)
    {
        isPlaying = false;
        isPreparing = false;
        ShowOffScreen();
    }

    /// <summary>
    /// 视频播放出错
    /// </summary>
    void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError("视频播放错误: " + message);
        isPlaying = false;
        isPreparing = false;
        ShowOffScreen();
    }

    /// <summary>
    /// 显示黑屏材质
    /// </summary>
    void ShowOffScreen()
    {
        if (screenRenderer != null && offMaterial != null)
        {
            screenRenderer.material = offMaterial;
        }
    }

    /// <summary>
    /// 显示播放材质
    /// </summary>
    void ShowPlayingScreen()
    {
        if (screenRenderer != null && playingMaterial != null)
        {
            screenRenderer.material = playingMaterial;
        }
    }
}