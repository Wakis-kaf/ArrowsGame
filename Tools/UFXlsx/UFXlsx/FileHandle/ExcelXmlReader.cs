using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UFXlsx.FileHandle
{
    public class ExcelXmlReader
    {
        private static ExcelXmlReader m_Instance;
        public static ExcelXmlReader Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new ExcelXmlReader();
                return m_Instance;
            }
        }
        private XmlDocument m_CurXmlDoc;
        private XmlNode m_CurXmlRootNode;
        public XmlNode BeginDecode(string xmlPath)
        {
            m_CurXmlDoc = new XmlDocument();
            m_CurXmlDoc.Load(xmlPath);
            m_CurXmlRootNode = m_CurXmlDoc.SelectSingleNode("root");
            return m_CurXmlRootNode;
        }
        public XmlNode Decode(string xmlPath)
        {
            var doc = new XmlDocument();
            doc.Load(xmlPath);
            var node = m_CurXmlDoc.SelectSingleNode("root");
            return node;
        }
        public void EndDecode()
        {
            m_CurXmlDoc = null;
            m_CurXmlRootNode = null;
        }

        public string ReadNodeValue(string nodeName)
        {
            if(m_CurXmlRootNode == null)
                throw new NotImplementedException();
            XmlNode node = m_CurXmlRootNode.SelectSingleNode(nodeName);
            return node?.InnerText;
        }
    }
}
