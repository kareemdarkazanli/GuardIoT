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

	public GameObject SnortDataContainer;
	public GameObject SnortDataPrefab;

	AmazonDynamoDBClient client;


	// Use this for initialization
	void Start () {
		UnityInitializer.AttachToGameObject (this.gameObject);
		var credentials = new BasicAWSCredentials("AKIAJIFWBWNXDKUAHRWA", "nDRoggu6qHBO4Do2Qh6Gdr/laope1XK0YDAr3s5y");		
		client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USWest2);
		RetrieveSnortData ();
	}

	public void displaySnortData(List<SnortData> allSnortData)
	{

		foreach (Transform childTransform in SnortDataContainer.transform)
		{
			Destroy(childTransform.gameObject);
		}

		var i = 0;

		foreach (SnortData snortData in allSnortData)
		{
			GameObject option = (GameObject)GameObject.Instantiate (SnortDataPrefab);
			option.transform.SetParent(SnortDataContainer.transform, false);

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
