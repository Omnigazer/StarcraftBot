//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.5
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace SWIG.BWAPIC {

using System;
using System.Runtime.InteropServices;
using BWAPI;

public class bwapiclient {
  public static Client BWAPIClient {
    set {
      bwapiclientPINVOKE.BWAPIClient_set(Client.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = bwapiclientPINVOKE.BWAPIClient_get();
      Client ret = (cPtr == global::System.IntPtr.Zero) ? null : new Client(cPtr, false);
      return ret;
    } 
  }

}

}
