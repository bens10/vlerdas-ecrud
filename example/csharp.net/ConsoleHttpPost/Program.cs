﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleHttpPost {
    class Program {
        static void Main(string[] args) {
            //string loadFileName = @"E:\VA\VLERDoc.xml";
            //string loadFileName = @"E:\Code\GitHub\vlerdas-ecrud\test\attachments\eCFT1MBAttachEmbeded.xml";
            //string loadFileName = @"C:\vler-proto-out\generated_out\xmlWithEmbeddedB64_5MB_a.xml"; //6.8 MB
            //string loadFileName = @"C:\vler-proto-out\generated_out\xmlWithEmbeddedB64_14MB_a.xml"; //19.1 MB
            //string loadFileName = @"C:\vler-proto-out\generated_out\xmlWithEmbeddedB64_17MB_a.xml"; //23.2 MB
            string loadFileName = @"C:\vler-proto-out\generated_out\xmlWithEmbeddedB64_21MB_a.xml"; //28.7 MB
            //string loadFileName = @"C:\vler-proto-out\generated_out\xmlWithEmbeddedB64_25MB_a.xml"; //34.2 MB
            byte[] xmlData = System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(loadFileName, Encoding.UTF8));
            //string strURL = "http://das.dynalias.org:8080/ecrud/v1/core/electronicCaseFiles/transform";
            //string strURL = "http://localhost:3001/ecrud/v1/core/electronicCaseFiles/transform";
            //for HTTPS
            //string strURL = "https://goldvler.va.gov/ecrud/v1/core/electronicCaseFiles/transform";
            //string strURL = "https://silvervler.va.gov/ecrud/v1/core/electronicCaseFiles/transform";
            //strURL += "?batchComplete=true";
            //string strURL = "https://silvervler.va.gov/ecrud/v1/core/electronicCaseFiles/transform?batchComplete=true&moroni=true";
            string strURL = "https://goldvler.va.gov/ecrud/v1/core/electronicCaseFiles/transform?batchComplete=true&moroni=true";
            try {
                HttpWebRequest POSTRequest = (HttpWebRequest)WebRequest.Create(strURL);
                //For HTTPS two way cert, must be imported to "Trusted Root Certificate Authorities", Location: IE > Tools > Internet Options > Content > Certificates
                POSTRequest.ClientCertificates.Add(new X509Certificate(@"E:\VA\2waySSL\clientcert.pkcs12", "keypass")); // ours/CACI


                //POSTRequest.ClientCertificates.Add(new X509Certificate(@"F:\Downloads\ides-cftdeom.asmr.com.cer", "test123"));
                //POSTRequest.ClientCertificates.Add(new X509Certificate(@"E:\VA\DoDCerts\dodRootCA2.cer"));
                //POSTRequest.ClientCertificates.Add(new X509Certificate(@"E:\VA\DoDCerts\DODJITCCA-27.crt"));
                //POSTRequest.ClientCertificates.Add(new X509Certificate(@"E:\VA\DoDCerts\ides-cftdemo.asmr.com.crt"));
                //End HTTPS
                POSTRequest.Method = "POST";
                POSTRequest.KeepAlive = false;
                POSTRequest.Timeout = 600000;
                //Content length of message body
                POSTRequest.Accept = "application/xml";

                var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
                POSTRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                boundary = "--" + boundary;

                using (var requestStream = POSTRequest.GetRequestStream()) {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    string fileName = "some_fileName.xml";
                    buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", "file", fileName, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", "text/xml", Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);

                    requestStream.Write(xmlData, 0, xmlData.Length);

                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);

                    var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
                    requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
                }

                HttpWebResponse POSTResponse = (HttpWebResponse)POSTRequest.GetResponse();
                StreamReader reader = new StreamReader(POSTResponse.GetResponseStream(), Encoding.UTF8);
                string daLocation = POSTResponse.GetResponseHeader("Location");
                HttpStatusCode daStatusCode = POSTResponse.StatusCode;

                string ResponseFromPost = reader.ReadToEnd().ToString();
                Console.WriteLine("StatusCode: " + daStatusCode);
                System.Diagnostics.Debug.WriteLine("StatusCode: " + daStatusCode);

                if (daStatusCode == HttpStatusCode.Accepted ||
                    daStatusCode == HttpStatusCode.Created ||
                    daStatusCode == HttpStatusCode.OK) {
                    Console.Write(ResponseFromPost);
                    System.Diagnostics.Debug.WriteLine(ResponseFromPost);
                }
            } catch (WebException we) {
                String errMsg = we.Message;
                if (we.Response != null)
                    errMsg += "; " + new StreamReader(we.Response.GetResponseStream(), Encoding.UTF8).ReadToEnd().ToString();
                System.Diagnostics.Debug.WriteLine(errMsg);
                Console.Write(errMsg);
            }
            Console.In.Read();
        }
    }
}
