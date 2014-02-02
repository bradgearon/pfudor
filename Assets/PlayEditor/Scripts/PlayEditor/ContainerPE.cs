#if UNITY_EDITOR
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


[XmlRoot("Collection")]
public class ContainerPE 
{
	[XmlArray("Items"),XmlArrayItem("Item")]
	public List <Item> Items = new List<Item>();
	
	public Setting _Setting = new Setting();
	
	public void Save (string path)
 	{
 		XmlSerializer serializer = new XmlSerializer(typeof(ContainerPE));
 		FileStream stream = new FileStream(path, FileMode.Create);
 		serializer.Serialize(stream, this);
		stream.Flush();
		stream.Close();
 	}
 
 	public static ContainerPE Load (string path)
 	{
 		XmlSerializer serializer = new XmlSerializer(typeof(ContainerPE));
		using (FileStream stream = new FileStream(path, FileMode.Open))
		{
			return serializer.Deserialize(stream) as ContainerPE;
		}
 	}
}

public class Item
{
	[XmlAttribute("name")]
	public string Name;
	
	public int ID;
	
	public float pX;
	public float pY;
	public float pZ;
	
	public float rX;
	public float rY;
	public float rZ;
	public float rW;
	
	public float sX;
	public float sY;
	public float sZ;
}

public class Setting
{
	public float camPX;
	public float camPY;
	public float camPZ;
	
	public float camRX;
	public float camRY;
	public float camRZ;
	public float camRW;
	
	public float mX;
	public float mY;
	
	public float bSize;
	public float minRScale;
	public float maxRScale;
	public float bForce;
	public float delSec;
	
	public int cMode;
	public int cSelectM;
	public int cAxis;
	public int cSpace;
	public int cBrushM;
	
	public bool useScaleToBrush;
	public bool useDelOnImP;
	public bool useShowSelected;
}
#endif