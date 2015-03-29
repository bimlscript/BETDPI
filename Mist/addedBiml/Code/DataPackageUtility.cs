using System;
using System.Data;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Collections.Generic;

[Serializable]
public class DataPackage
{
	[DataMember]
	public string Name { get; set; }
	
	[DataMember]
	public ICollection<DataPackageResource> Resources { get; set; }	
	
	[DataMember]
	public string License { get; set; }
	
	public static DataPackage Load(string json)
	{
		var serializer = new JavaScriptSerializer();
		var dataPackage = serializer.Deserialize<DataPackage>(json);
		return dataPackage;
	}
}

[Serializable]
public class DataPackageResource
{
	[DataMember]
	public string Url { get; set; }
	
	[DataMember]
	public string Path { get; set; }
	
	[DataMember]
	public DataPackageResourceSchema Schema { get; set; }
}

[Serializable]
public class DataPackageResourceSchema
{
	[DataMember]
	public ICollection<DataPackageResourceSchemaField> Fields { get; set; }
}

[Serializable]
public class DataPackageResourceSchemaField
{
	[DataMember]
	public string Name { get; set; }
	
	[DataMember]
	public string Type { get; set; }
	
	public int Length
	{
		get
		{
			if (DbType == DbType.String)
			{
				return 4000;
			}
			
			return 0;
		}
	}
	
	public int Precision
	{
		get
		{
			if (DbType == DbType.Decimal)
			{
				return 18;
			}
			
			return -1;
		}
	}
	
	public int Scale
	{
		get
		{
			if (DbType == DbType.Decimal)
			{
				return 0;
			}
			
			return -1;
		}
	}
	
	public DbType DbType
	{
		get
		{
			if (string.IsNullOrEmpty(Type))
			{
				return DbType.String;
			}
			
			if (string.Compare("boolean", Type.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
			{
				return DbType.Boolean;
			}
			
			if (string.Compare("integer", Type.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
			{
				return DbType.Int32;
			}

			if (string.Compare("number", Type.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
			{
				return DbType.Decimal;
			}

			if (string.Compare("string", Type.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
			{
				return DbType.String;
			}

			throw new NotSupportedException("The Type '" + Type + "' is not supported presently.");
		}
	}
}
