using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.XR.WSA.WebCam;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using System.Text;


#if !UNITY_EDITOR
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
#endif

public class PhotoCaptureExample : MonoBehaviour
{
    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture;
    public RectTransform canvasRectTransform;
    public TextUpdater textUpdater = null;
    //public GameObject flicky;
    //public GameObject labelText;

    string filename;
    string filePath;

    bool useColorFormat = false;
    bool useTexture = false;
    //int cycleCount = 0;
    //bool sendFinished = true;

    float startFreq = 8;
    float freqResolution = 1f;
    float STIMULATION_DURATION = 4f;


    float targetTime = 0.0f;
    int state;

    int MAX_OBJECT_NUM = 3;
    bool CONTINUAL = false;
    bool picture_taken = false;

    StimulusManager stimulusManager;

    // Use this for initialization
    void Start()
    {
        filename = "tmp.jpg";
        filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Create a PhotoCapture object
        var stimulusManagerObject = GameObject.Find("Stimulus Manager");
        stimulusManager = stimulusManagerObject.GetComponent<StimulusManager>();

        targetTime = 1f;
        state = 0;

        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            CameraParameters cameraParameters = new CameraParameters();
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = cameraResolution.width;
            cameraParameters.cameraResolutionHeight = cameraResolution.height;
            cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

            // Activate the camera
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate (PhotoCapture.PhotoCaptureResult result) {
                // Take a picture
                //photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
                //photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);

            });
        });
    }

    private void Update()
    {
        /*
        if (++cycleCount >= 2 && sendFinished == true)
        {
            if (photoCaptureObject != null)
            {
                //photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
                photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
            }

            cycleCount = 0;
        }*/

        targetTime -= Time.deltaTime;

        if (targetTime <= 0.0f)
        {
            if (state == 0)
            {
                if (CONTINUAL)
                {
                    photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
                    targetTime = 5f; //timeout time
                    state = -1;
                }
                else
                {
                    if (!picture_taken)
                    {
                        photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
                        picture_taken = true;
                    }
                    stimulusManager.GazeShift();
                    targetTime = 0.5f;
                    state = 1;
                }
                
            }

            else if(state == -1)
            {
                state = 0;
            }

            else if(state == 1)
            {
                stimulusManager.StartStimulating();
                targetTime = STIMULATION_DURATION;
                state = 2;
            }
            else if(state == 2)
            {
                stimulusManager.StopStimulating();
                state = 0;
            }
        }


    }

    private void OnApplicationQuit()
    {
        if (photoCaptureObject != null)
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
#if !UNITY_EDITOR
        //sendFinished = false;
        byte[] jpg_array = File.ReadAllBytes(filePath);
        //Debug.Log("Send TCP message");
        SendMessage(jpg_array);
#endif
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }

        if (!CONTINUAL)
        {
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }

    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (useTexture) {
            // Copy the raw image data into the target texture
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);

            // Deactivate the camera
            //photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);

            //gameObjDisplayPhoto.GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", targetTexture);
            //Debug.Log("Set Texture!");

            //Debug.Log(targetTexture.height.ToString());
            //Debug.Log(targetTexture.width.ToString());


#if !UNITY_EDITOR
        byte[] jpg_array = targetTexture.EncodeToJPG();
        //Debug.Log("Send TCP message");
        SendMessage(jpg_array);
#endif
        }

        if (useColorFormat) {
            List<byte> imageBufferList = new List<byte>();
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

            // In this example, we captured the image using the BGRA32 format.
            // So our stride will be 4 since we have a byte for each rgba channel.
            // The raw image data will also be flipped so we access our pixel data
            // in the reverse order.
            int stride = 4;
            float denominator = 1.0f / 255.0f;
            List<Color> colorArray = new List<Color>();
            for (int i = imageBufferList.Count - 1; i >= 0; i -= stride)
            {
                float a = (int)(imageBufferList[i - 0]) * denominator;
                float r = (int)(imageBufferList[i - 1]) * denominator;
                float g = (int)(imageBufferList[i - 2]) * denominator;
                float b = (int)(imageBufferList[i - 3]) * denominator;

                colorArray.Add(new Color(r, g, b, a));
            }

            //targetTexture.SetPixels(colorArray.ToArray());
            //targetTexture.Apply();
        }
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown the photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    void ParseObjectDetectionResult(int read_length, byte[] result_bytes)
    {
        if (read_length < 26)
        {
            Debug.Log("No detected object. Lenght: " + read_length.ToString());
            return;
        }
        else
        {
            float panelWidth = canvasRectTransform.rect.width;// * canvasRectTransform.localScale.x;
            float panelHeight = canvasRectTransform.rect.height;// * canvasRectTransform.localScale.y;

            var labelArray = new byte[10];
            var positionArray = new float[4];

            // Show no more than MAX_OBJECT_NUM objects
            read_length = Math.Min(MAX_OBJECT_NUM * 26, read_length);

            Vector3[] positions = new Vector3[read_length / 26];
            Vector3[] qs = new Vector3[read_length / 26];
            float[] freqs = new float[read_length / 26];
            string[] labels = new string[read_length / 26];
            for (int i = 0; i < read_length; i += 26)
            {
                System.Buffer.BlockCopy(result_bytes, i, labelArray, 0, 10);
                string label = Encoding.UTF8.GetString(labelArray);
                Debug.Log(label);

                System.Buffer.BlockCopy(result_bytes, i + 10, positionArray, 0, 16);
                Debug.Log(positionArray[0].ToString() + " " + positionArray[1].ToString());

                float pos_x = panelWidth * ((positionArray[0] - 0.5f) * 1.3f + 0.5f);
                float pos_y = panelWidth * (1 - positionArray[1]) - 200;
                float pos_z = 100f;
                Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(pos_x, pos_y, pos_z));
                positions[i / 26] = pos;
                qs[i / 26] = new Vector3(-90, 0, 0);
                freqs[i / 26] = startFreq + (i / 26) * freqResolution;
                labels[i / 26] = label;
            }

            if (textUpdater != null)
                textUpdater.setTargetLabelMap(labels);
            stimulusManager.InitialTargets(positions, qs, freqs, labels);
            stimulusManager.GazeShift();
            targetTime = 0.5f;
            state = 1;
            return;
        }

        /*        
        var floatArray = new float[(read_length) / 4];
        System.Buffer.BlockCopy(result_bytes, 0, floatArray, 0, read_length);

        float[] result = floatArray;
        if (result.Length < 4)
        {
            Debug.Log("No detected object " + result_bytes[0].ToString());
            state = 0;
            targetTime = 0;
            return;
        }

        float panelWidth = canvasRectTransform.rect.width;// * canvasRectTransform.localScale.x;
        float panelHeight = canvasRectTransform.rect.height;// * canvasRectTransform.localScale.y;

        Vector3[] positions = new Vector3[(int)result.Length / 4];
        Quaternion[] qs = new Quaternion[(int)result.Length / 4];
        float[] freqs = new float[(int)result.Length / 4];

        for (int i = 0; i < result.Length; i += 4)
        {
            Debug.Log("Object " + (i / 4).ToString() + ": x = " + result[i] + ", y = " + result[i + 1]);
            float pos_x = panelWidth * ((result[i] - 0.5f) * 1.3f + 0.5f);
            float pos_y = panelWidth * (1 - result[i + 1]) - 200;
            float pos_z = 100f;
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(pos_x, pos_y, pos_z));
            positions[i / 4] = pos;
            qs[i / 4] = Quaternion.Euler(-90, 0, 0);
            freqs[i / 4] = startFreq + (i / 4) * freqResolution;
        }
        stimulusManager.InitialTargets(positions, qs, freqs);
        stimulusManager.StopStimulating();
        targetTime = 0.5f;
        state = 1;
        //photoCaptureObject.Dispose();
        */
    }

#if !UNITY_EDITOR
 public async void SendMessage(byte[] bmsg)
 {
     HostName hostName;
     string port = "9999";
     try
     {
         hostName = new HostName("192.168.1.100");
     }
     catch (ArgumentException)
     {
         return;
     }
     StreamSocket socket;
     try
     {
         using (socket = new StreamSocket())
         {
             await socket.ConnectAsync(hostName, port);
             DataWriter dw = new DataWriter(socket.OutputStream);
             //dw.WriteString(msg);
             dw.WriteBytes(bmsg);
             await dw.StoreAsync();
             //Debug.Log("Write bytes");
             Stream streamIn = socket.InputStream.AsStreamForRead();
             
             byte[] result_bytes = new byte[500];
             int read_length = await streamIn.ReadAsync(result_bytes, 0, 500);
             Debug.Log("Read Length " + read_length.ToString());
             if (read_length >= 0){
                 //var floatArray = new float[read_length / 4];
                 //System.Buffer.BlockCopy(result_bytes, 0, floatArray, 0, read_length);
                 //Debug.Log(string.Join("", floatArray));
                 //ParseObjectDetectionResult(floatArray);
                 ParseObjectDetectionResult(read_length, result_bytes);
                 socket.Dispose();
            }
         }
     }
     catch (Exception e)
    {
        Debug.Log(e.ToString());
    }
 }
#endif
}