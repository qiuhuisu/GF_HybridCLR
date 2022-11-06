using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityGameFramework.Runtime;

public class WebRequestCert : UnityEngine.Networking.CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        //bool validation = base.ValidateCertificate(certificateData);

        //if (!validation)
        //{
        //    X509Certificate2 certificate = new X509Certificate2(certificateData);
        //    // Do custom validation that puts it's result into the validation boolean.
        //}
        return true;
    }
}
