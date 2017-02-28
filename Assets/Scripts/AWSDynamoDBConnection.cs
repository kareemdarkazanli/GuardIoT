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

	private int currentTag = 0;

	AmazonDynamoDBClient client;


	// Use this for initialization
	void Start () {
		UnityInitializer.AttachToGameObject (this.gameObject);
		var credentials = new BasicAWSCredentials("AKIAJIFWBWNXDKUAHRWA", "nDRoggu6qHBO4Do2Qh6Gdr/laope1XK0YDAr3s5y");		
		client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USWest2);
		RetrieveSensorTag ();

	}

	public void displaySensorData(List<SensorTag> allSensorTags){
		foreach (Transform childTransform in SensorTagPanel.transform)
		{
			Destroy(childTransform.gameObject);
		}

		GameObject accX = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		accX.transform.SetParent(SensorTagDataPanel.transform, false);
		accX.transform.GetComponentInChildren<Text> ().text = "\t" + allSensorTags[currentTag].accx;

		GameObject accY = (GameObject)GameObject.Instantiate (SensorTagDataPrefab);
		accY.transform.SetParent(SensorTagDataPanel.transform, false);
		accY.transform.GetComponentInChildren<Text> ().text = "\t" + allSensorTags[currentTag].accy;


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

	private void RetrieveSensorTag()
	{
		var request = new ScanRequest
		{
			TableName = "SensorTag",
		};

		client.ScanAsync (request, (result) => {
			List<SensorTag> allSensorTags = new List<SensorTag>();


			foreach (var item in result.Response.Items)
			{

				SensorTag sensorTag = new SensorTag();
				foreach (var kvp in item)
				{
					string attributeName = kvp.Key;
					AttributeValue value = kvp.Value;
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
											sensorTag.batterytemsp = k.Key + ": " + valu.N;

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
			displaySensorData(allSensorTags);
		});
	}

	private void RetrieveSnortData()
	{
		var request = new ScanRequest
		{
			TableName = "Snort",
		};

		client.ScanAsync(request,(result)=>{
			List<SnortData> allSnortData = new List<SnortData>();
			foreach (var item in result.Response.Items)
			{
				SnortData snortData = new SnortData();

				//Debug.Log(item.Count);
				foreach (var kvp in item)
				{
					string attributeName = kvp.Key;
					AttributeValue value = kvp.Value;
					if(attributeName == "sensor")
					{
						snortData.sensor = value.S;
						//Debug.Log(snortData.sensor);
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
			displaySnortData(allSnortData);
		});
	}

	#endregion


		
}
