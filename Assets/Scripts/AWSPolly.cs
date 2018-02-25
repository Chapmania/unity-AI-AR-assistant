using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using UnityEngine.Networking;

public class AWSPolly : MonoBehaviour {

	public AmazonPollyClient client;

	public string cognitoCredentials;
	public RegionEndpoint region = RegionEndpoint.USEast2;
	public string speech = "Hello world! This is a sample text used to test AWSPolly for Unity 2017.3.0. Hope this help you guys";
	public string voice = "Matthew";

	private string audio_path;

	void Awake(){
		UnityInitializer.AttachToGameObject(gameObject);
		Amazon.AWSConfigs.HttpClient = Amazon.AWSConfigs.HttpClientOption.UnityWebRequest;
	}
	
	void Start () {


		CognitoAWSCredentials credentials = new CognitoAWSCredentials ( cognitoCredentials, RegionEndpoint.USEast2 );
		client = new AmazonPollyClient(credentials, RegionEndpoint.USEast2 );
		
		// Create speech synthesis request.
		SynthesizeSpeechRequest synthesizeSpeechPresignRequest = new SynthesizeSpeechRequest();
		synthesizeSpeechPresignRequest.Text = speech;
		synthesizeSpeechPresignRequest.VoiceId = voice;
		synthesizeSpeechPresignRequest.OutputFormat = OutputFormat.Ogg_vorbis;
		// Get the presigned URL for synthesized speech audio stream.
		client.SynthesizeSpeechAsync(synthesizeSpeechPresignRequest, (responseObject) =>
		{
			audio_path = Application.persistentDataPath + "" + (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds + ".mp3";
			//audio_path = Application.dataPath + "/Audios/Generated/" + (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds + ".mp3";

			using (FileStream fileStream = File.Create(audio_path))
			{

				CopyTo(responseObject.Response.AudioStream,fileStream);
				// fileStream.Flush();
				// fileStream.Close();
			}
			
			StartCoroutine(PlayAudioClip(audio_path));
		});
	}
	
	IEnumerator PlayAudioClip(string audio_path)
	{
		using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + audio_path, AudioType.OGGVORBIS))
		{
			yield return www.Send();

			if (www.isNetworkError)
				print(www.error);
			else{
				AudioClip audio = DownloadHandlerAudioClip.GetContent(www);
				GetComponent<AudioSource>().clip = audio;
				GetComponent<AudioSource>().Play();
			}
		}
	}

	public static void CopyTo(Stream input, Stream outputAudio)
	{
		byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
		int bytesRead;

		while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
		{
				outputAudio.Write(buffer, 0, bytesRead);
		}
	}

}
