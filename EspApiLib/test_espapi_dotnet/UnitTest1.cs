using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Hornbill;

namespace espapi_dotnet_tests;

public class UnitTest1
{
    
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    // instance name or full server URL
    private string instanceUrl = "MyInstance"; // Or, i.e "http://localhost/sw/";
    private const string userId = "testUser";
    private const string password = "password";
    private const string xmlmc = "xmlmc";
    private const string dav = "dav";
    private string apiKey = string.Empty;

    private XmlmcService xmlmcService = null;

    public UnitTest1()
    {
        xmlmcService = new XmlmcService(instanceUrl, xmlmc, dav, apiKey);
        // resolve the instance name
        if (instanceUrl.ToLower().StartsWith("http"))
        {
            Assert.True(string.Compare(instanceUrl, xmlmcService.ServerURL) == 0);
        }
        else
        {
            Assert.True(string.Compare(instanceUrl, xmlmcService.ServerURL) != 0);
            instanceUrl = xmlmcService.ServerURL;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            xmlmcService.AddParam("userId", userId);
            xmlmcService.AddParam("password", password).EncodeValue(XmlmcEncoding.Base64);
            xmlmcService.Invoke("session", "userLogon");
            string sessionId = xmlmcService.GetResponseParamAsString("sessionId");
            Assert.NotNull(sessionId);
        }
    }

    [Fact]
    public void TestPutFileWithTextContents()
    {
        string filePath = "/uprof/test.txt";
        string contents = "My XML file";
        xmlmcService.PutText(filePath, contents);
        Assert.True(xmlmcService.DoesFileExists(filePath));
        string fileContents = xmlmcService.GetFileContents(filePath);
        Assert.True(contents.CompareTo(fileContents) == 0);
        xmlmcService.RemoveFile(filePath);
    }

    [Fact]
    public void TestPutFile()
    {   
        string localFilePath = Path.GetTempFileName();
        string remoteFilePath = "/uprof/testputfile.txt";
        string contents = "12345678901234567890123456789012345678901234567890123456789012345678901234567890";
        using (FileStream fs = File.Create(localFilePath, contents.Length))
        {
            fs.Write(System.Text.UTF8Encoding.UTF8.GetBytes(contents), 0, contents.Length);
        }

        xmlmcService.PutFile(localFilePath, remoteFilePath);
        
        File.Delete(localFilePath);
        Assert.True(xmlmcService.DoesFileExists(remoteFilePath));
        string fileContents = xmlmcService.GetFileContents(remoteFilePath);
        Assert.True(contents.CompareTo(fileContents) == 0);
        MemoryStream memoryStream;
        xmlmcService.GetFile(remoteFilePath, out memoryStream);
        if (memoryStream != null)
        {
            fileContents = System.Text.UTF8Encoding.UTF8.GetString(memoryStream.ToArray());
            Assert.True(contents.CompareTo(fileContents) == 0);
            memoryStream.Close();
        }
        xmlmcService.RemoveFile(remoteFilePath);
    }

    [Fact]
    public void TestAddParam()
    {
        // attribute
        // string
        xmlmcService.AddParam("key","string");
        Assert.True(xmlmcService.GetParamsXML().CompareTo("<key>string</key>") == 0);
        xmlmcService.ClearParams();
        // boolean
        xmlmcService.AddParam("key", false);
        Assert.True(xmlmcService.GetParamsXML().CompareTo("<key>false</key>") == 0);
        xmlmcService.ClearParams();
        xmlmcService.AddParam("key", true);
        Assert.True(xmlmcService.GetParamsXML().CompareTo("<key>true</key>") == 0);
        xmlmcService.ClearParams();
        // double
        xmlmcService.AddParam("key", (double)0.12345);
        Assert.True(xmlmcService.GetParamsXML().CompareTo("<key>0.12345</key>") == 0);
        xmlmcService.ClearParams();
        // long
        xmlmcService.AddParam("key", (long)12345);
        Assert.True(xmlmcService.GetParamsXML().CompareTo("<key>12345</key>") == 0);
        xmlmcService.ClearParams();
        // float
        xmlmcService.AddParam("key", (float)0.12345);
        Assert.True(xmlmcService.GetParamsXML().CompareTo("<key>0.12345</key>") == 0);
        xmlmcService.ClearParams();
        // int
        xmlmcService.AddParam("key", (int)12345);
        Assert.True(xmlmcService.GetParamsXML().CompareTo("<key>12345</key>") == 0);
        xmlmcService.ClearParams();
        // DateTime
        DateTime dt = DateTime.UtcNow;
        xmlmcService.AddParam("key", dt);
        string s = xmlmcService.GetParamsXML();
        Assert.True(xmlmcService.GetParamsXML().CompareTo(String.Format("<key>{0}</key>", dt.ToString("yyyy-MM-dd HH:mm:ssZ"))) == 0);// 2016-05-27 11:02:58Z
        xmlmcService.ClearParams();
        // Param
        XmlmcParam param = new XmlmcParam("child", "string");
        xmlmcService.AddParam("key", param);
        Assert.True(xmlmcService.GetParamsXML().CompareTo("<key><child>string</child></key>") == 0);
        xmlmcService.ClearParams();
        // List of params
        List <XmlmcParam> lstParams = new List<XmlmcParam>();
        lstParams.Add(new XmlmcParam("child1", "string"));
        lstParams.Add(new XmlmcParam("child2", "string"));
        lstParams.Add(new XmlmcParam("child3", "string"));
        xmlmcService.AddParam("key", lstParams);
        Assert.True(xmlmcService.GetParamsXML().CompareTo("<key><child1>string</child1><child2>string</child2><child3>string</child3></key>") == 0);
        xmlmcService.ClearParams();
        // List of objects
        List<String> lstStrings = new List<String>();
        lstStrings.Add("string1");
        lstStrings.Add("string2");
        lstStrings.Add("string3");
        xmlmcService.AddParamIdList<String>("key", lstStrings);
        Assert.True(xmlmcService.GetParamsXML().CompareTo("<key>string1</key><key>string2</key><key>string3</key>") == 0);
        xmlmcService.ClearParams();
    }

    [Fact]
    public void TestCopyFile()
    {
        string remoteFilePathFrom = "/uprof/testcopyfile1.temp";
        string remoteFilePathTo = "/uprof/testcopyfile2.temp";

        string contents = "My XML text";
        xmlmcService.PutText(remoteFilePathFrom, contents);
        Assert.True(xmlmcService.DoesFileExists(remoteFilePathFrom));

        xmlmcService.CopyFile(remoteFilePathFrom, remoteFilePathTo);

        Assert.True(xmlmcService.DoesFileExists(remoteFilePathTo));
        Assert.True(xmlmcService.DoesFileExists(remoteFilePathFrom));

        xmlmcService.RemoveFile(remoteFilePathTo);
        xmlmcService.RemoveFile(remoteFilePathFrom);
        Assert.False(xmlmcService.DoesFileExists(remoteFilePathFrom));
        Assert.False(xmlmcService.DoesFileExists(remoteFilePathTo));
    }

    [Fact]
    public void TestMoveFile()
    {
        string remoteFilePathFrom = "/uprof/testmovefile1.temp";
        string remoteFilePathTo = "/uprof/testmovefile2.temp";

        string contents = "My XML text";
        xmlmcService.PutText(remoteFilePathFrom, contents);
        Assert.True(xmlmcService.DoesFileExists(remoteFilePathFrom));

        xmlmcService.MoveFile(remoteFilePathFrom, remoteFilePathTo);
        Assert.True(xmlmcService.DoesFileExists(remoteFilePathTo));
        Assert.False(xmlmcService.DoesFileExists(remoteFilePathFrom));

        xmlmcService.RemoveFile(remoteFilePathTo);
        Assert.False(xmlmcService.DoesFileExists(remoteFilePathTo));
    }

    [Fact]
    public void TestDoesFileExists()
    {
        string remoteFilePath = "/uprof/testdoesfileexists.temp";
        Assert.False(xmlmcService.DoesFileExists(remoteFilePath));

        string contents = "My XML text";
        xmlmcService.PutText(remoteFilePath, contents);

        Assert.True(xmlmcService.DoesFileExists(remoteFilePath));

        xmlmcService.RemoveFile(remoteFilePath);
        Assert.False(xmlmcService.DoesFileExists(remoteFilePath));
    }

    [Fact]
    public void TestDoesFolderExists()
    {
        string remoteFilePath = "/uprof/testdoesfolderexists";
        Assert.True(xmlmcService.DoesFileExists(remoteFilePath));

        xmlmcService.CreateFolder(remoteFilePath);

        Assert.True(xmlmcService.DoesFileExists(remoteFilePath));

        xmlmcService.RemoveFile(remoteFilePath);
        Assert.False(xmlmcService.DoesFileExists(remoteFilePath));
    }

    [Fact]
    public void TestCreateDeleteFolder()
    {
        string remoteFilePath = "/uprof/testcreatefolder";
        xmlmcService.CreateFolder(remoteFilePath);
        Assert.True(xmlmcService.DoesFileExists(remoteFilePath));
        xmlmcService.RemoveFile(remoteFilePath);
        Assert.False(xmlmcService.DoesFileExists(remoteFilePath));
    }

    [Fact]
    public void TestDeleteFile()
    {
        string remoteFilePath = "/uprof/testdoesfileexists.temp";
        // If file doesn't exists, it should fail sliently
        xmlmcService.RemoveFile(remoteFilePath);

        string contents = "My XML text";
        xmlmcService.PutText(remoteFilePath, contents);

        Assert.True(xmlmcService.DoesFileExists(remoteFilePath));

        xmlmcService.RemoveFile(remoteFilePath);
        Assert.False(xmlmcService.DoesFileExists(remoteFilePath));
    }

    [Fact]
    public void TestResolveInstanceName()
    {
        Uri uri = xmlmcService.ResolveInstanceName("hornbill");
        Assert.NotNull(uri);
        Assert.True(uri.AbsoluteUri.CompareTo("hornbill") != 0);

        uri = xmlmcService.ResolveInstanceName("https://live.hornbill.com/hornbill");
        Assert.True(uri.AbsoluteUri.CompareTo("https://live.hornbill.com/hornbill") == 0);
    }

    [Fact]
    public void TestTrailingSlashForServiceName()
    {
        xmlmcService.ClearParams();

        //This returns the error for supportworks without trailing slash. Orion works fine
        xmlmcService.Invoke("mail", "checkServiceAvailability");
    }

    [Fact]
    public void TestReturnParamAsCountAndArray()
    {
        xmlmcService.ClearParams();


        xmlmcService.AddParam("sentenceText", "Hornbill ! £ $ % ^  * ( ) _ + @ ~ . , / ' ' < > & \" 'Supportworks'"); //Testing (xml) special characters
        xmlmcService.AddParam("language", "en-GB");
        xmlmcService.AddParam("suggestWords", "true");
        xmlmcService.Invoke("utility", "spellCheck");

        long count = xmlmcService.GetResponseParamCount("spellCheckErrorItem");
        Assert.Equal(count, 2);

        XmlDocument xmld = xmlmcService.GetResponseParamAsComplexType("spellCheckErrorItem", 2);
        XmlNodeList nList = xmld.GetElementsByTagName("errorWord");
        XmlNode xNode = nList.Item(0);
        Assert.Equal(xNode.FirstChild.Value, "Supportworks");

        List<string> list = xmlmcService.GetResponseParamAsStringArray("spellCheckErrorItem");
        Assert.Equal(list.Count, 0);
    }

    [Fact]
    public void TestBuildXMLAndComplexType()
    {
        xmlmcService.ClearParams();
        //setDiagnosticsLevel with complex type params

        xmlmcService.AddParam("severityLevel","all");
        List<XmlmcParam> paramList = new List<XmlmcParam>();
        paramList.Add(new XmlmcParam("general", "true"));
        paramList.Add(new XmlmcParam("system", "true"));
        paramList.Add(new XmlmcParam("process", "true"));
        paramList.Add(new XmlmcParam("security", "true"));
        paramList.Add(new XmlmcParam("comms", "true"));
        paramList.Add(new XmlmcParam("database", "true"));
        paramList.Add(new XmlmcParam("sql", "true"));
        paramList.Add(new XmlmcParam("perf", "true"));
        paramList.Add(new XmlmcParam("scripts", "true"));

        xmlmcService.AddParam("logMessageGroup", paramList);

        // come back here
        //mc.AddParam(doc);
        xmlmcService.AddParam("enableResultXmlSchemaValidation", "true");
        xmlmcService.AddParam("enableDatabaseSecurityHinting", "true");

        string xml = xmlmcService.GetInvokeXML("session", "setDiagnosticsLevel");
        System.Diagnostics.Debug.Print(xml);
        xmlmcService.Invoke("session", "setDiagnosticsLevel");
        xml = xmlmcService.GetResponseXML();

    }

    [Fact]
    public void TestReturnXML()
    {
        xmlmcService.ClearParams();

        xmlmcService.Invoke("session", "getSessionInfo");

        Assert.False(xmlmcService.GetResponseParamAsBool("isGuestSession"));

        XmlDocument xmld = xmlmcService.GetResponseXMLDocument();
        XmlNodeList nList = xmld.GetElementsByTagName("sessionId");

        nList = xmld.GetElementsByTagName("createdOn");
        DateTime createdOn = new DateTime();
        createdOn = DateTime.ParseExact(nList.Item(0).InnerText, "yyyy-MM-dd HH:mm:ssZ", null);

        xmld = xmlmcService.GetResponseParamAsComplexType("currentLanguage", 0);
        nList = xmld.GetElementsByTagName("language");
        Assert.Equal(nList.Item(0).InnerText, "en-GB");

        DateTime date = xmlmcService.GetResponseParamAsTime("createdOn");
        Assert.Equal(date, createdOn);
    }

    [Fact]
    public void TestException()
    {
        xmlmcService.ClearParams();
        try
        {
            xmlmcService.Invoke("session", "userLogn");
        }
        catch (RequestFailureException e)
        {
            Assert.NotNull(e);
            Assert.Equal(xmlmcService.GetLastResponseErrorMessage(), "Operation handler not found: session::userLogn");
        }
    }

    [Fact]
    public void Test_i18n()
    {
        xmlmcService.ClearParams();
        xmlmcService.AddParam("sentenceText", "árvízhhvh tükör fúrógép");
        xmlmcService.AddParam("language", "hu-HU");
        xmlmcService.AddParam("suggestWords", "true");

        xmlmcService.Invoke("utility", "spellCheck");
        XmlDocument xmld = xmlmcService.GetResponseParamAsComplexType("spellCheckErrorItem", 1);
        XmlNodeList nList = xmld.ChildNodes;
        Assert.Equal(nList.Count, 1);
        Assert.Equal(nList.Item(0).ChildNodes.Count, 5);

        Assert.Equal(nList.Item(0).ChildNodes.Item(0).InnerText, "árvízhhvh");
        Assert.Equal(nList.Item(0).ChildNodes.Item(1).InnerText, "árvíz");
        Assert.Equal(nList.Item(0).ChildNodes.Item(2).InnerText, "árvaházak");
        Assert.Equal(nList.Item(0).ChildNodes.Item(3).InnerText, "árvaház");
        Assert.Equal(nList.Item(0).ChildNodes.Item(4).InnerText, "árvalányhaj");

    }

    [Fact]
    public void TestLargeString()
    {
        xmlmcService.ClearParams();
        xmlmcService.AddParam("sentenceText", "Support works ESP was developed as an ASCII based system defaulting to multi-byte encoding within the fat clients and no consideration for Unicode in any of the server components except for where integration points required it. As Support works expands use within multi-national companies the requirement for proper multi-language support has become a common theme. As a result various engineering changes have been made working towards a full Unicode enabled product. The following outlines the capability both current and expected showing the road map for full Unicode enable within the Support works ESP platform. Support works ESP was developed as an ASCII based system defaulting to multi-byte encoding within the fat clients and no consideration for Unicode in any of the server components except for where integration points required it. As Support works expands use within multi-national companies the requirement for proper multi-language support has become a common theme. As a result various engineering changes have been made working towards a full Unicode enabled product. The following outlines the capability both current and expected showing the road map for full Unicode enable within the Support works ESP platform. Support works ESP was developed as an ASCII based system defaulting to multi-byte encoding within the fat clients and no consideration for Unicode in any of the server components except for where integration points required it. As Support works expands use within multi-national companies the requirement for proper multi-language support has become a common theme. As a result various engineering changes have been made working towards a full Unicode enabled product. The following outlines the capability both current and expected showing the road map for full Unicode enable within the Support works ESP platform. Support works ESP was developed as an ASCII based system defaulting to multi-byte encoding within the fat clients and no consideration for Unicode in any of the server components except for where integration points required it. As Support works expands use within multi-national companies the requirement for proper multi-language support has become a common theme. As a result various engineering changes have been made working towards a full Unicode enabled product. The following outlines the capability both current and expected showing the road map for full Unicode enable within the Support works ESP platform. Support works ESP was developed as an ASCII based system defaulting to multi-byte encoding within the fat clients and no consideration for Unicode in any of the server components except for where integration points required it. As Support works expands use within multi-national companies the requirement for proper multi-language support has become a common theme. As a result various engineering changes have been made working towards a full Unicode enabled product. The following outlines the capability both current and expected showing the road map for full Unicode enable within the Support works ESP platform. Support works ESP was developed as an ASCII based system defaulting to multi-byte encoding within the fat clients and no consideration for Unicode in any of the server components except for where integration points required it. As Support works expands use within multi-national companies the requirement for proper multi-language support has become a common theme. As a result various engineering changes have been made working towards a full Unicode enabled product. The following outlines the capability both current and expected showing the road map for full Unicode enable within the Support works ESP platform. Support works ESP was developed as an ASCII based system defaulting to multi-byte encoding within the fat clients and no consideration for Unicode in any of the server components except for where integration points required it. As Support works expands use within multi-national companies the requirement for proper multi-language support has become a common theme. As a result various engineering changes have been made working towards a full Unicode enabled product. The following outlines the capability both current and expected showing the road map for full Unicode enable within the Support works ESP platform. Support works ESP was developed as an ASCII based system defaulting to multi-byte encoding within the fat clients and no consideration for Unicode in any of the server components except for where integration points required it. As Support works expands use within multi-national companies the requirement for proper multi-language support has become a common theme. As a result various engineering changes have been made working towards a full Unicode enabled product. The following outlines the capability both current and expected showing the road map for full Unicode enable within the Support works ESP platform.");
        xmlmcService.AddParam("language", "en-GB");
        xmlmcService.AddParam("suggestWords", "true");

        xmlmcService.Invoke("utility", "spellCheck");
        XmlDocument xmld = xmlmcService.GetResponseParamAsComplexType("spellCheckErrorItem", 1);
        XmlNodeList nList = xmld.ChildNodes;
        Assert.Equal(nList.Count, 0);

    }

    [Fact]
    public void TestGetRecord()
    {
        xmlmcService.ClearParams();

        xmlmcService.AddParam("participant", "urn:sys:user:admin");
        xmlmcService.AddParam("content", "Hello from admin!");
        //try
        //{
            xmlmcService.Invoke("activity", "conversationStart");
        //}
        //catch(System.Net.WebException ex)
        //{
        //    Assert.AreEqual(ex.Message, "You can not specify yourself as a participant, this is done automatically");
        //}
        
        string convId = xmlmcService.GetResponseParamAsString("conversationId");

        xmlmcService.AddParam("conversationId", convId);
        xmlmcService.AddParam("content", "hi from admin - msg1");
        xmlmcService.Invoke("activity", "conversationPost");
        string msgId = xmlmcService.GetResponseParamAsString("messageId");

        xmlmcService.AddParam("table", "h_buzc_conversations");

        xmlmcService.AddParam("keyValue", new XmlmcParam("h_id", convId));
        xmlmcService.AddParam("formatValues", true);
        xmlmcService.AddParam("returnRawValues", true);

        string xml = xmlmcService.GetInvokeXML("data", "getRecord");
        xmlmcService.Invoke("data", "getRecord");

        XmlDocument xmld = xmlmcService.GetResponseParamAsComplexType("recordData", 0);
        XmlNodeList nList = xmld.GetElementsByTagName("h_id");
        Assert.Equal(nList.Item(0).FirstChild.Value, convId);

        xmlmcService.AddParam("conversationId", convId);
        xmlmcService.Invoke("activity", "conversationDelete");

    }

    [Fact]
    public void TestComplexType()
    {
        xmlmcService.ClearParams();
        XmlmcParam param = xmlmcService.AddParam("location");
        param.Add("latitude", 51.5575);
        param.Add("longitude", 0.4026);
        param.Add("elevation", 0);
        param.Add("placeName", "Odyssey Business Park");
        param.Add("timestamp", DateTime.Now.ToUniversalTime());

        string xml = xmlmcService.GetParamsXML();
        xmlmcService.Invoke("session", "locationRegisterCurrent");
        string results = xmlmcService.GetResponseXML();
        Assert.NotNull(results);
    }

    [Fact]
    public void TestXMLEncoding()
    {
        xmlmcService.ClearParams();

        List<string> contents = new List<string>();
        /*Add the xml encoding strings here for testing
         * test will stop on first failure
         */
        contents.Add("<>&'\"\"");
        contents.Add("&amp; &lt; &gt; &apos; &quot;");
        contents.Add("<>&'\"\" &amp; &lt; &gt; &apos; &quot;");
        contents.Add("<&'\"\" &a<>&mp; &lt; &gt; &ap<os; &q>uot; &quot;");
        contents.Add("a->b");
        contents.Add("a -&gt; b");
        foreach(string content in contents)
        {
            //post message to my buzz
            xmlmcService.AddParam("socialObjectRef", string.Format("urn:sys:user:{0}", userId));
            xmlmcService.AddParam("content", content);
            xmlmcService.Invoke("activity", "postMessage");
            string activityID = xmlmcService.GetResponseParamAsString("activityID");
            //query it
            xmlmcService.AddParam("activityID", activityID);
            xmlmcService.Invoke("activity", "activityStreamQueryItem");
            XmlDocument xmld = xmlmcService.GetResponseParamAsComplexType("activity", 1);
            XmlNodeList nList = xmld.GetElementsByTagName("content");
            string returnedContent = nList.Item(0).FirstChild.Value;
            Assert.Equal(content, returnedContent);
        }
    }
}