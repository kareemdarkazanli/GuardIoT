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

	public GameObject ScrollView;

	public GameObject SensorTag;
	public GameObject Rpi;

	private int currentTag = 0;

	private SensorTag currentSensorTag;

	AmazonDynamoDBClient client;

	List<SnortData> allSnortData;

	List<GameObject> sensorTags = new List<GameObject>();
	List<GameObject> sensorData = new List<GameObject>();


	// Use this for initialization
	void Start () {
		ScrollView.GetComponent<ScrollRect> ().content = SensorTagDataPanel.GetComponent<RectTransform>();
		UnityInitializer.AttachToGameObject (this.gameObject);
		var credentials = new BasicAWSCredentials("AKIAJIFWBWNXDKUAHRWA", "nDRoggu6qHBO4Do2Qh6Gdr/laope1XK0YDAr3s5y");		
		client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USWest2);
		RetrieveRaspberryPis ();
	}

	void Update(){
		
	}


	public void SpawnRpi(){

	}

	public void SpawnSensorTags(List<RaspberryPi> allRaspberryPis){

		GameObject _Rpi = Instantiate (Rpi, new Vector3 (0, 0, 0), Quaternion.Euler(0, 0, 0)) as GameObject;
		_Rpi.transform.GetChild(0).GetComponentInChildren<Button> ().GetComponentInChildren<Text> ().text = allRaspberryPis[0].defRoute;
		_Rpi.transform.GetChild (0).GetChild (1).GetComponentInChildren<Text> ().text = allSnortData[0].alert;
		Debug.Log(allRaspberryPis[0].defRoute);
		_Rpi.GetComponent<Renderer> ().material.color = Color.red;

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
			_SensorTag.transform.GetChild(0).GetComponentInChildren<Button> ().GetComponentInChildren<Text> ().text = allRaspberryPis[0].sensorTags [i].name;
			_SensorTag.transform.GetChild (0).GetChild (1).GetComponentInChildren<Text> ().text = sensortag.airtemp;
			_SensorTag.transform.GetChild (0).GetChild (2).GetComponentInChildren<Text> ().text = sensortag.airpressure;
			_SensorTag.transform.GetChild (0).GetChild (3).GetComponentInChildren<Text> ().text = sensortag.batterytemp;
			_SensorTag.GetComponent<Renderer> ().material.color = Color.red;
			_SensorTag.transform.GetChild (1).GetComponent<ParticleSystem> ().GetComponent<ParticleMovement>().target = _Rpi.transform;
			float rotate = i * (360 / 12) + 100;
			_SensorTag.transform.GetChild (1).GetComponent<ParticleSystem> ().transform.rotation = Quaternion.Euler(0, rotate, 0);

			_SensorTag.transform.GetChild(0).GetComponentInChildren<Button> ().onClick.AddListener(delegate () {
				Debug.Log(sensortag.tagNumber);
				currentTag = sensortag.tagNumber;
				currentSensorTag = sensortag;
				displayCanvas();
				displaySensorData();
			});

			sensorTags.Add (_SensorTag);
			Debug.Log (cosOfAngle + "," + sinOfAngle);
			Debug.Log (sensortag.name + " :" + i * (360/12));
			i++;

		}

		InvokeRepeating("getData", 0f, 0.01f);


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
				
				Debug.Log(raspberryPi.defRoute);
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

		Debug.Log ("Here");
		foreach (Transform childTransform in SensorTagPanel.transform)
		{
			Destroy(childTransform.gameObject);
		}

		GameObject accX = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		accX.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (accX);
		//accX.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.accx;

		GameObject accY = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		accY.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (accY);

		//accY.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.accy;

		GameObject accZ = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		accZ.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (accZ);

		//accZ.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.accz;

		GameObject airpressure = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		airpressure.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (airpressure);

		//airpressure.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.airpressure;

		GameObject airtemp = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		airtemp.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (airtemp);

		//airtemp.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.airtemp;

		GameObject ambienttemp = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		ambienttemp.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (ambienttemp);

		//ambienttemp.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.ambienttemp;


		GameObject batterytemp = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		batterytemp.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (batterytemp);

		//batterytemp.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.batterytemp;


		GameObject batteryvolt = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		batteryvolt.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (batteryvolt);

		//batteryvolt.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.batteryvolt;

		GameObject defroute = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		defroute.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (defroute);

		//defroute.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.defroute;

		GameObject gyrox = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		gyrox.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (gyrox);

		//gyrox.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.gyrox;

		GameObject gyroy = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		gyroy.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (gyroy);

		//gyroy.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.gyroy;



		GameObject gyroz = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		gyroz.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (gyroz);

		//gyroz.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.gyroz;


		GameObject hdchumidity = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		hdchumidity.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (hdchumidity);

		//hdchumidity.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.hdchumidity;


		GameObject hdctemp = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		hdctemp.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (hdctemp);

		//hdctemp.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.hdctemp;



		GameObject light = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		light.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (light);

		//light.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.light;



		GameObject name = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		name.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (name);

		//name.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.name;



		GameObject objecttemp = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		objecttemp.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (objecttemp);

		//objecttemp.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.objecttemp;



		GameObject rssi = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		rssi.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (rssi);

		//rssi.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.rssi;



		GameObject seq = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		seq.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (seq);

		//seq.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.seq;



		GameObject uptime = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		uptime.transform.SetParent(SensorTagDataPanel.transform, false);
		sensorData.Add (uptime);

		//uptime.transform.GetComponentInChildren<Text> ().text = "\t" + sensorTag.uptime;


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

			Debug.Log (i + ": " +snortData.sensor);


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
		public string alertClass { get; set; }
		[DynamoDBProperty]
		public int alertPriority { get; set; }
		[DynamoDBProperty]
		public string destinationIP { get; set; }
		[DynamoDBProperty]
		public int destinationPort { get; set; }
		[DynamoDBProperty]
		public string eventTimeStamp { get; set; }
		[DynamoDBProperty]
		public int protocol { get; set; }
		[DynamoDBProperty]
		public string sourceIP { get; set; }
		[DynamoDBProperty]
		public int sourcePort { get; set; }
	}


	#endregion


	#region retrieve dynamoDB data

	private void RetrieveRaspberryPis()
	{
		Debug.Log ("Here");
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
						Debug.Log(rpi.defRoute);

					}
					else if(attributeName == "myName"){
						rpi.myName = value.S;
						Debug.Log(rpi.myName);

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
											sensorTag.name = k.Key + ": " + valu.S;

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
			Debug.Log(allSensorTags.Count);
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
											sensorTag.name = k.Key + ": " + valu.S;

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
			rpi.sensorTags = allSensorTags;
			allRaspberryPis.Add(rpi);
			if(!canvas.isActiveAndEnabled){
				updateSensorTags(allRaspberryPis);
			}
			else{
				//displaySensorData(currentSensorTag);
				test(allRaspberryPis);
			}
		});
	}

	public void test(List<RaspberryPi> allRaspberryPis){
		sensorData[0].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].accx;
		Debug.Log ("\t" + allRaspberryPis[0].sensorTags [currentTag].accx);
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
		//Debug.Log (SensorTagDataPanel.transform.GetChild (12).GetComponentInChildren<Text> ().text);

		sensorData[16].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].objecttemp;
		sensorData[17].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].rssi;
		sensorData[18].transform.GetComponentInChildren<Text> ().text = "\t" + allRaspberryPis[0].sensorTags [currentTag].seq;
		sensorData[19].transform.GetComponentInChildren<Text> ().text =  "\t" + allRaspberryPis[0].sensorTags [currentTag].uptime;


	}

	public void updateSensorTags(List<RaspberryPi> allRaspberryPis){
		if (allRaspberryPis [0].sensorTags.Count == sensorTags.Count) {
			int i = 0;
			foreach(GameObject sensorTag in sensorTags){
				sensorTag.transform.GetChild (0).GetChild (1).GetComponentInChildren<Text> ().text = allRaspberryPis[0].sensorTags [i].airtemp;
				sensorTag.transform.GetChild (0).GetChild (2).GetComponentInChildren<Text> ().text = allRaspberryPis[0].sensorTags [i].airpressure;
				sensorTag.transform.GetChild (0).GetChild (3).GetComponentInChildren<Text> ().text = allRaspberryPis[0].sensorTags [i].batterytemp;
				i++;
			}
			//print ("updated");
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
						Int32.TryParse(value.N, out snortData.order);
						Debug.Log("order added");

					}
					else if(attributeName == "alertPriority"){
						Int32.TryParse(value.N, out snortData.alertPriority);
						Debug.Log("alertPriority added");

					}
					else if(attributeName == "destinationPort"){
						Int32.TryParse(value.N, out snortData.destinationPort);
						Debug.Log("destinationPort added");

					}
					else if(attributeName == "protocol"){
						Int32.TryParse(value.N, out snortData.protocol);
						Debug.Log("protocol added");

					}
					else if(attributeName == "sourcePort"){
						Int32.TryParse(value.N, out snortData.sourcePort);
						Debug.Log("sourcePort added");

					}
					else if(attributeName == "alert")
					{
						snortData.alert = value.S;
						Debug.Log("alert added");

					}
					else if(attributeName == "alertClass")
					{
						snortData.alertClass = value.S;
						Debug.Log("alertClass added");

					}
					else if(attributeName == "destinationIP")
					{
						snortData.destinationIP = value.S;
						Debug.Log("destinationIP added");

					}
					else if(attributeName == "eventTimeStamp")
					{
						snortData.eventTimeStamp = value.S;
						Debug.Log("eventTimeStamp added");

					}
					else if(attributeName == "sourceIP")
					{
						snortData.sourceIP = value.S;
						Debug.Log("sourceIP added");

					}

				}
				allSnortData.Add(snortData);
			}
			SpawnSensorTags(allRaspberryPis);

		});
	}

	#endregion


		
}
