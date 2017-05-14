using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.CognitoIdentity;
using Amazon.Runtime;
using Amazon;
using Amazon.Runtime.Internal;
using Amazon.Util;
using UnityEngine.UI;
using System;
using System.Threading;
using Amazon.DynamoDBv2.Model;


public class AWSDynamoDBConnection : MonoBehaviour {

	public string IdentityPoolId = "";
	public string CognitoPoolRegion = RegionEndpoint.USWest2.SystemName;

	private AWSCredentials credentials;
	private AmazonDynamoDBClient dynamoClient;

	public string DynamoRegion = RegionEndpoint.USWest2.SystemName;
	private static IAmazonDynamoDB _ddbClient;
	private IAmazonDynamoDB _client;
	private DynamoDBContext _context;

	public GameObject SensorTagDataPanel;
	public GameObject SensorTagDataPrefab;
	public GameObject SensorTagPanel;
	public GameObject SensorTagPrefab;
	public GameObject RaspberryPiPanel;
	public GameObject RaspberryPiPrefab;
	public Canvas canvas;
	public GameObject _Rpi;

	public GameObject ScrollView;

	public GameObject SensorTag;
	public GameObject Rpi;

	private int currentTag = 0;

	private SensorTag currentSensorTag;

	AmazonDynamoDBClient client;

	List<SnortData> allSnortData;
	List<SensorTag> allSensorTag;


	List<GameObject> sensorTags = new List<GameObject>();
	List<GameObject> sensorData = new List<GameObject>();
	float timer = 90.0f;

	private Color currentColor;
	bool startTimer = false;


	// Use this for initialization
	void Start () {
		ScrollView.GetComponent<ScrollRect> ().content = SensorTagDataPanel.GetComponent<RectTransform>();
		UnityInitializer.AttachToGameObject (this.gameObject);
		var credentials = new BasicAWSCredentials("UserName", "Password");		
		client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USWest2);
		RetrieveRaspberryPis ();
	}

	void Update(){
		if (startTimer) {
			timer -= Time.deltaTime;
		}
	}


	public void SpawnRpi(){

	}

	public void SpawnSensorTags(List<RaspberryPi> allRaspberryPis){

		_Rpi = Instantiate (Rpi, new Vector3 (0, 0, 0), Quaternion.Euler(0, 0, 0)) as GameObject;
		_Rpi.transform.GetChild(0).GetComponentInChildren<Button> ().GetComponentInChildren<Text> ().text = allSnortData[0].sensor;
		_Rpi.transform.GetChild (0).GetChild (1).GetComponentInChildren<Text> ().text = "System is safe";
		_Rpi.GetComponent<Renderer> ().material.color = Color.green;
		currentColor = Color.green;

		float scew = 1.0f;
		float offset = 0f;
		int i = 0;
		foreach(SensorTag sensortag in  allRaspberryPis[0].sensorTags) {
			if (i % 12 == 0) {
				scew += 1.0f;
				offset += 30.0f;
			}
			var sinOfAngle =  scew * 270 * Mathf.Sin(i * (360/12) * Mathf.Deg2Rad + offset);
			var cosOfAngle = scew * 270 * Mathf.Cos(i * (360/12) * Mathf.Deg2Rad + offset);

			GameObject _SensorTag = Instantiate (SensorTag, new Vector3 (sinOfAngle, 0, cosOfAngle), Quaternion.Euler(0, 0, 0)) as GameObject;
			_SensorTag.transform.GetChild(0).GetComponentInChildren<Button> ().GetComponentInChildren<Text> ().text = allRaspberryPis[0].sensorTags [i].shortName;
			_SensorTag.transform.GetChild (0).GetChild (1).GetComponentInChildren<Text> ().text = sensortag.airtemp;
			_SensorTag.transform.GetChild (0).GetChild (2).GetComponentInChildren<Text> ().text = sensortag.airpressure;
			_SensorTag.transform.GetChild (0).GetChild (3).GetComponentInChildren<Text> ().text = sensortag.seq;
			_SensorTag.GetComponent<Renderer> ().material.color = Color.green;
			_SensorTag.transform.GetChild (1).GetComponent<ParticleSystem> ().GetComponent<ParticleMovement>().target = _Rpi.transform;
			float rotate = i * (360 / 12) + 100;
			_SensorTag.transform.GetChild (1).GetComponent<ParticleSystem> ().transform.rotation = Quaternion.Euler(0, rotate, 0);
			var em = _SensorTag.transform.GetChild (1).GetComponent<ParticleSystem> ().emission;
			em.enabled = false;
			//em.enabled = true;

			_SensorTag.transform.GetChild(0).GetComponentInChildren<Button> ().onClick.AddListener(delegate () {
				currentTag = sensortag.tagNumber;
				currentSensorTag = sensortag;
				displayCanvas();
				displaySensorData();
			});

			sensorTags.Add (_SensorTag);
			i++;

		}

		InvokeRepeating("getData", 0f, 2f);
		InvokeRepeating ("updateSystemColor", 0f, 5f);

	}

	public void countdown(){
		if (timer == 0) {
			timer = 30;
		} else if (timer == 30) {
			timer = 0;
		}
	}
		
	public void updateSystemColor(){
		SnortData oldSnort = new SnortData();
		oldSnort.eventTimeStamp = allSnortData [0].eventTimeStamp;

		var request = new ScanRequest
		{
			TableName = "Snort",
		};

		client.ScanAsync(request,(result)=>{
			allSnortData = new List<SnortData>();
			foreach (var item in result.Response.Items)
			{
				SnortData snortData = new SnortData();

				foreach (var kvp in item)
				{
					string attributeName = kvp.Key;
					AttributeValue value = kvp.Value;
					if(attributeName == "sensor")
					{
						snortData.sensor = value.S;
					}
					else if(attributeName == "order"){
						snortData.order = value.S;
					}
					else if(attributeName == "alert")
					{
						snortData.alert = value.S;

					}
					else if(attributeName == "alertPriority"){
						snortData.alertPriority = value.S;
					}
					else if(attributeName == "eventTimeStamp")
					{
						snortData.eventTimeStamp = value.S;
					}
				}
				allSnortData.Add(snortData);
			}
			if(oldSnort.eventTimeStamp == allSnortData[0].eventTimeStamp){
				if(timer < 0){
					_Rpi.GetComponent<Renderer> ().material.color = Color.green;
					foreach(GameObject sensorTag in sensorTags){
						//sensorTag.GetComponent<Renderer> ().material.color = Color.green;
						sensorTag.GetComponent<Renderer> ().material.color = Color.green;
					}
					timer = 90.0f;
					startTimer = false;
					_Rpi.transform.GetChild (0).GetChild (1).GetComponentInChildren<Text> ().text = "System is safe";

				}
				//_Rpi.GetComponent<Renderer> ().material.color = Color.green;
				/*_Rpi.GetComponent<Renderer> ().material.color = currentColor;
				foreach(GameObject sensorTag in sensorTags){
					//sensorTag.GetComponent<Renderer> ().material.color = Color.green;
					sensorTag.GetComponent<Renderer> ().material.color = currentColor;
				}*/
			}
			else if (allSnortData[0].alertPriority == "4"){
				_Rpi.GetComponent<Renderer> ().material.color = Color.green;
				foreach(GameObject sensorTag in sensorTags){
					sensorTag.GetComponent<Renderer> ().material.color = Color.green;
				}
				currentColor = Color.green;
				_Rpi.transform.GetChild (0).GetChild (1).GetComponentInChildren<Text> ().text = allSnortData[0].alert;
				timer = 90.0f;
				startTimer = true;

			}
			else if(allSnortData[0].alertPriority == "3"){
				_Rpi.GetComponent<Renderer> ().material.color = Color.yellow;
				foreach(GameObject sensorTag in sensorTags){
					sensorTag.GetComponent<Renderer> ().material.color = Color.yellow;

				}
				currentColor = Color.yellow;
				_Rpi.transform.GetChild (0).GetChild (1).GetComponentInChildren<Text> ().text = allSnortData[0].alert;
				timer = 90.0f;
				startTimer = true;


			}
			else if(allSnortData[0].alertPriority == "2"){
				_Rpi.GetComponent<Renderer> ().material.color = Color.red;
				foreach(GameObject sensorTag in sensorTags){
					sensorTag.GetComponent<Renderer> ().material.color = Color.red;

				}
				currentColor = Color.red;
				_Rpi.transform.GetChild (0).GetChild (1).GetComponentInChildren<Text> ().text = allSnortData[0].alert;
				timer = 90.0f;
				startTimer = true;


			}
			else {
				_Rpi.GetComponent<Renderer> ().material.color = Color.black;
				foreach(GameObject sensorTag in sensorTags){
					sensorTag.GetComponent<Renderer> ().material.color = Color.black;

				}
				currentColor = Color.black;
				_Rpi.transform.GetChild (0).GetChild (1).GetComponentInChildren<Text> ().text = allSnortData[0].alert;
				timer = 90.0f;
				startTimer = true;


			}

		});

	}

	public void displayRaspberryPis(List<RaspberryPi> allRaspberryPis){
		

		foreach (Transform childTransform in RaspberryPiPanel.transform)
		{
			Destroy(childTransform.gameObject);
		}
		foreach (RaspberryPi raspberryPi in allRaspberryPis)
		{
			GameObject rpi = (GameObject)GameObject.Instantiate (RaspberryPiPrefab);
			rpi.transform.SetParent(RaspberryPiPanel.transform, false);
			rpi.transform.GetChild(0).GetComponent<Text>().text = raspberryPi.defRoute;
			rpi.GetComponent<Button>().onClick.AddListener(delegate () {
				
			});
		}

	}

	public void displayCanvas(){
		canvas.gameObject.SetActive (true);
	}

	public void hideCanvas(){
		canvas.gameObject.SetActive (false);
	}

	public void displaySensorData(){

		foreach (Transform childTransform in SensorTagPanel.transform)
		{
			Destroy(childTransform.gameObject);
		}

		GameObject accX = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		accX.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (accX);

		GameObject accY = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		accY.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (accY);

		GameObject accZ = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		accZ.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (accZ);

		GameObject airpressure = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		airpressure.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (airpressure);

		GameObject airtemp = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		airtemp.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (airtemp);

		GameObject ambienttemp = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		ambienttemp.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (ambienttemp);

		GameObject batterytemp = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		batterytemp.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (batterytemp);

		GameObject batteryvolt = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		batteryvolt.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (batteryvolt);

		GameObject defroute = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		defroute.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (defroute);

		GameObject gyrox = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		gyrox.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (gyrox);

		GameObject gyroy = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		gyroy.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (gyroy);


		GameObject gyroz = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		gyroz.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (gyroz);


		GameObject hdchumidity = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		hdchumidity.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (hdchumidity);


		GameObject hdctemp = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		hdctemp.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (hdctemp);


		GameObject light = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		light.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (light);


		GameObject name = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		name.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (name);

		GameObject objecttemp = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		objecttemp.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (objecttemp);


		GameObject rssi = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		rssi.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (rssi);


		GameObject seq = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		seq.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (seq);


		GameObject uptime = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		uptime.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (uptime);

	}

	public void displaySnortData(List<SnortData> allSnortData)
	{

		foreach (Transform childTransform in SensorTagDataPanel.transform)
		{
			Destroy(childTransform.gameObject);
		}

		var i = 0;

		foreach (SnortData snortData in allSnortData)
		{
			GameObject option = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
			option.transform.SetParent(SensorTagDataPanel.transform, false);



			//Debug.Log (snortData.sensor);
			option.transform.GetChild(0).GetComponent<Text>().text = i + ": " +snortData.sensor;
			i++;
			//
			/*if (optionNode.optionNodes.Count == 0) {
				option.GetComponent<Button>().onClick.AddListener(delegate () {
					Debug.Log(optionNode.assetName);
				});

			} else {
				option.GetComponent<Button>().onClick.AddListener(delegate () { displayModelNameLevel(optionNode, optionNode.optionNodes); });
			}

			//Instantiate options
			if (optionNode.title == "Images") {
				displayModelNameLevel(optionNode, optionNode.optionNodes);
			}*/
		}

	}

	#region dynamoDB object definition
	[DynamoDBTable("Snort")]
	public class Snort
	{
		[DynamoDBHashKey]   // Hash key.
		public string sensor { get; set; }
		[DynamoDBProperty]
		public int order { get; set; }
		[DynamoDBProperty]
		public string alert { get; set; }
		[DynamoDBProperty]
		public int alertPriority { get; set; }
		[DynamoDBProperty]
		public string eventTimeStamp { get; set; }
	}


	#endregion


	#region retrieve dynamoDB data

	private void RetrieveRaspberryPis()
	{
		var request = new ScanRequest
		{
			TableName = "SensorTag",
		};

		client.ScanAsync (request, (result) => {
			List<RaspberryPi> allRaspberryPis = new List<RaspberryPi>();
			RaspberryPi rpi = new RaspberryPi();
			List<SensorTag> allSensorTags = new List<SensorTag>();
			int i = 0;

			foreach (var item in result.Response.Items)
			{
				SensorTag sensorTag = new SensorTag();
				sensorTag.tagNumber = i;

				foreach (var kvp in item)
				{
					string attributeName = kvp.Key;
					AttributeValue value = kvp.Value;
					if(attributeName == "defRoute"){
						rpi.defRoute = value.S;

					}
					else if(attributeName == "myName"){
						rpi.myName = value.S;

					}
					if(value.IsMSet){
						foreach(var key in value.M.Keys){

							foreach(var val in value.M.Values){
								if(val.IsMSet){
									foreach(var k in val.M){
										AttributeValue valu;
										val.M.TryGetValue(k.Key, out valu);

										if(k.Key.Trim() == "Air Pressure (hPa)"){
											sensorTag.airpressure = k.Key + ": " + valu.N;
											//Debug.Log(sensorTag.airpressure);
									
										}
										else if(k.Key.Trim() == "RSSI (dBm)"){
											sensorTag.rssi = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.rssi);

										}
										else if(k.Key.Trim() == "Light (lux)"){
											sensorTag.light = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.light);

										}
										else if(k.Key.Trim() == "Seq #"){
											sensorTag.seq = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.seq);

										}
										else if(k.Key.Trim() == "Air Temp (C)"){
											sensorTag.airtemp = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.airtemp);

										}
										else if(k.Key.Trim() == "Object Temp (C)"){
											sensorTag.objecttemp = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.objecttemp);

										}
										else if(k.Key.Trim() == "Acc Y (G)"){
											sensorTag.accy = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.accy);

										}
										else if(k.Key.Trim() == "Gyro Z (deg per sec)"){
											sensorTag.gyroz = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.gyroz);

										}
										else if(k.Key.Trim() == "Gyro X (deg per sec)"){
											sensorTag.gyrox = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.gyrox);
										}
										else if(k.Key.Trim() == "HDC Humidity (%RH)"){
											sensorTag.hdchumidity = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.hdchumidity);

										}
										else if(k.Key.Trim() == "Uptime (sec)"){
											sensorTag.uptime = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.uptime);

										}
										else if(k.Key.Trim() == "defRoute"){
											sensorTag.defroute = k.Key + ": " + valu.S;

											//Debug.Log(sensorTag.defroute);

										}
										else if(k.Key.Trim() == "Battery Temp (C)"){
											sensorTag.batterytemp = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.batterytemsp);

										}
										else if(k.Key.Trim() == "HDC Temp (C)"){
											sensorTag.hdctemp = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.hdctemp);

										}
										else if(k.Key.Trim() == "Ambient Temp (C)"){
											sensorTag.ambienttemp = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.ambienttemp);

										}
										else if(k.Key.Trim() == "Gyro Y (deg per sec)"){
											sensorTag.gyroy = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.gyroy);

										}
										else if(k.Key.Trim() == "myName"){
											sensorTag.shortName = valu.S + "";
											sensorTag.name = k.Key + ": " + sensorTag.shortName;

											//Debug.Log(sensorTag.name);

										}
										else if(k.Key.Trim() == "Acc X (G)"){
											sensorTag.accx = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.accx);

										}
										else if(k.Key.Trim() == "Battery Volt (mV)"){
											sensorTag.batteryvolt = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.batteryvolt);

										}
										else if(k.Key.Trim() == "Acc Z (G)"){
											sensorTag.accz = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.accz);

										}
									}

								}

							}

						}
					}
				
				}
				allSensorTags.Add(sensorTag);
				i++;
			}
			rpi.sensorTags = allSensorTags;
			allRaspberryPis.Add(rpi);
			//displayRaspberryPis(allRaspberryPis);
			RetrieveSnortData(allRaspberryPis);
		});
	}

	public void getData(){
		var request = new ScanRequest
		{
			TableName = "SensorTag",
		};

		client.ScanAsync (request, (result) => {
			List<RaspberryPi> allRaspberryPis = new List<RaspberryPi>();
			RaspberryPi rpi = new RaspberryPi();
			List<SensorTag> allSensorTags = new List<SensorTag>();

			foreach (var item in result.Response.Items)
			{
				SensorTag sensorTag = new SensorTag();

				foreach (var kvp in item)
				{
					string attributeName = kvp.Key;
					AttributeValue value = kvp.Value;
					if(attributeName == "defRoute"){
						rpi.defRoute = value.S;

					}
					else if(attributeName == "myName"){
						rpi.myName = value.S;

					}
					if(value.IsMSet){
						foreach(var key in value.M.Keys){

							foreach(var val in value.M.Values){
								if(val.IsMSet){
									foreach(var k in val.M){
										AttributeValue valu;
										val.M.TryGetValue(k.Key, out valu);

										if(k.Key.Trim() == "Air Pressure (hPa)"){
											sensorTag.airpressure = k.Key + ": " + valu.N;
											//Debug.Log(sensorTag.airpressure);

										}
										else if(k.Key.Trim() == "RSSI (dBm)"){
											sensorTag.rssi = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.rssi);

										}
										else if(k.Key.Trim() == "Light (lux)"){
											sensorTag.light = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.light);

										}
										else if(k.Key.Trim() == "Seq #"){
											sensorTag.seq = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.seq);

										}
										else if(k.Key.Trim() == "Air Temp (C)"){
											sensorTag.airtemp = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.airtemp);

										}
										else if(k.Key.Trim() == "Object Temp (C)"){
											sensorTag.objecttemp = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.objecttemp);

										}
										else if(k.Key.Trim() == "Acc Y (G)"){
											sensorTag.accy = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.accy);

										}
										else if(k.Key.Trim() == "Gyro Z (deg per sec)"){
											sensorTag.gyroz = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.gyroz);

										}
										else if(k.Key.Trim() == "Gyro X (deg per sec)"){
											sensorTag.gyrox = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.gyrox);
										}
										else if(k.Key.Trim() == "HDC Humidity (%RH)"){
											sensorTag.hdchumidity = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.hdchumidity);

										}
										else if(k.Key.Trim() == "Uptime (sec)"){
											sensorTag.uptime = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.uptime);

										}
										else if(k.Key.Trim() == "defRoute"){
											sensorTag.defroute = k.Key + ": " + valu.S;

											//Debug.Log(sensorTag.defroute);

										}
										else if(k.Key.Trim() == "Battery Temp (C)"){
											sensorTag.batterytemp = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.batterytemsp);

										}
										else if(k.Key.Trim() == "HDC Temp (C)"){
											sensorTag.hdctemp = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.hdctemp);

										}
										else if(k.Key.Trim() == "Ambient Temp (C)"){
											sensorTag.ambienttemp = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.ambienttemp);

										}
										else if(k.Key.Trim() == "Gyro Y (deg per sec)"){
											sensorTag.gyroy = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.gyroy);

										}
										else if(k.Key.Trim() == "myName"){
											sensorTag.shortName = valu.S + "";
											sensorTag.name = k.Key + ": " + sensorTag.shortName;

											//Debug.Log(sensorTag.name);

										}
										else if(k.Key.Trim() == "Acc X (G)"){
											sensorTag.accx = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.accx);

										}
										else if(k.Key.Trim() == "Battery Volt (mV)"){
											sensorTag.batteryvolt = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.batteryvolt);

										}
										else if(k.Key.Trim() == "Acc Z (G)"){
											sensorTag.accz = k.Key + ": " + valu.N;

											//Debug.Log(sensorTag.accz);

										}
									}

								}

							}

						}
					}

				}
				allSensorTags.Add(sensorTag);

			}
			List<SensorTag> oldSensorTags = rpi.sensorTags;
			rpi.sensorTags = allSensorTags;
			allRaspberryPis.Add(rpi);
			if(!canvas.isActiveAndEnabled){
				updateSensorTags(allRaspberryPis, oldSensorTags);
			}
			else{
				//displaySensorData(currentSensorTag);
				test(allRaspberryPis);
			}
		});
	}

	public void test(List<RaspberryPi> allRaspberryPis){
		sensorData[0].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].accx;
		sensorData[1].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].accy;
		sensorData[2].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].accz;

		//SensorTagDataPanel.transform.GetChild (0).GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].accx;
		//Debug.Log (SensorTagDataPanel.transform.GetChild (0).GetComponentInChildren<Text> ().text);
		sensorData[3].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].airpressure;
		//Debug.Log (SensorTagDataPanel.transform.GetChild (1).GetComponentInChildren<Text> ().text);

		sensorData[4].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].airtemp;
		//Debug.Log (SensorTagDataPanel.transform.GetChild (2).GetComponentInChildren<Text> ().text);

		sensorData[5].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].ambienttemp;
		//Debug.Log (SensorTagDataPanel.transform.GetChild (3).GetComponentInChildren<Text> ().text);

		sensorData[6].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].batterytemp;
		//Debug.Log (SensorTagDataPanel.transform.GetChild (4).GetComponentInChildren<Text> ().text);

		sensorData[7].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].batteryvolt;
		sensorData[8].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].defroute;

		sensorData[9].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].gyrox;
		sensorData[10].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].gyroy;
		sensorData[11].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].gyroz;
		sensorData[12].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].hdchumidity;
		sensorData[13].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].hdctemp;
		sensorData[14].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].light;
		sensorData[15].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].name;

		sensorData[16].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].objecttemp;
		sensorData[17].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].rssi;
		sensorData[18].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].seq;
		sensorData[19].transform.GetComponentInChildren<Text> ().text =  "\t" + allRaspberryPis[0].sensorTags [currentTag].uptime;


	}

	public void updateSensorTags(List<RaspberryPi> allRaspberryPis, List<SensorTag> oldSensorTags){
		if (allRaspberryPis [0].sensorTags.Count == sensorTags.Count) {
			int i = 0;

			foreach(GameObject sensorTag in sensorTags){
				if(sensorTag.transform.GetChild (0).GetChild (3).GetComponentInChildren<Text> ().text == allRaspberryPis[0].sensorTags [i].seq){
					var em = sensorTag.transform.GetChild (1).GetComponent<ParticleSystem> ().emission;
					em.enabled = false;	
					//em.enabled = true;				

				}
				else{
					var em = sensorTag.transform.GetChild (1).GetComponent<ParticleSystem> ().emission;
					em.enabled = true;				
				}
				sensorTag.transform.GetChild (0).GetChild (1).GetComponentInChildren<Text> ().text = allRaspberryPis[0].sensorTags [i].airtemp;
				sensorTag.transform.GetChild (0).GetChild (2).GetComponentInChildren<Text> ().text = allRaspberryPis[0].sensorTags [i].airpressure;
				sensorTag.transform.GetChild (0).GetChild (3).GetComponentInChildren<Text> ().text = allRaspberryPis[0].sensorTags [i].seq;

				i++;
			}
		}


	}

	private void RetrieveSnortData(List<RaspberryPi> allRaspberryPis)
	{
		var request = new ScanRequest
		{
			TableName = "Snort",
		};

		client.ScanAsync(request,(result)=>{
			allSnortData = new List<SnortData>();
			foreach (var item in result.Response.Items)
			{
				SnortData snortData = new SnortData();

				foreach (var kvp in item)
				{
					string attributeName = kvp.Key;
					AttributeValue value = kvp.Value;
					if(attributeName == "sensor")
					{
						snortData.sensor = value.S;
					}
					else if(attributeName == "order"){
						snortData.order = value.S;
					}
					else if(attributeName == "alert")
					{
						snortData.alert = value.S;

					}
					else if(attributeName == "alertPriority"){
						snortData.alertPriority = value.S;

					}
					else if(attributeName == "eventTimeStamp")
					{
						snortData.eventTimeStamp = value.S;

					}


				}
				allSnortData.Add(snortData);
			}
			Debug.Log(allSnortData.Count);
			SpawnSensorTags(allRaspberryPis);

		});
	}

	#endregion


		
}
