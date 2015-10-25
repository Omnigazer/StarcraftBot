//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 3.0.5
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------

namespace SWIG.BWAPI {

public partial class UnitTypeIntPair : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal UnitTypeIntPair(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(UnitTypeIntPair obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~UnitTypeIntPair() {
    Dispose();
  }

  public virtual void Dispose() {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          bwapiPINVOKE.delete_UnitTypeIntPair(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  
public override int GetHashCode()
{
   return this.swigCPtr.Handle.GetHashCode();
}

public override bool Equals(object obj)
{
    bool equal = false;
    if (obj is UnitTypeIntPair)
      equal = (((UnitTypeIntPair)obj).swigCPtr.Handle == this.swigCPtr.Handle);
    return equal;
}
  
public bool Equals(UnitTypeIntPair obj) 
{
    if (obj == null) return false;
    return (obj.swigCPtr.Handle == this.swigCPtr.Handle);
}

public static bool operator ==(UnitTypeIntPair obj1, UnitTypeIntPair obj2)
{
    if (object.ReferenceEquals(obj1, obj2)) return true;
    if (object.ReferenceEquals(obj1, null)) return false;
    if (object.ReferenceEquals(obj2, null)) return false;
   
    return obj1.Equals(obj2);
}

public static bool operator !=(UnitTypeIntPair obj1, UnitTypeIntPair obj2)
{
    if (object.ReferenceEquals(obj1, obj2)) return false;
    if (object.ReferenceEquals(obj1, null)) return true;
    if (object.ReferenceEquals(obj2, null)) return true;

    return !obj1.Equals(obj2);
}




  public UnitTypeIntPair() : this(bwapiPINVOKE.new_UnitTypeIntPair__SWIG_0(), true) {
  }

  public UnitTypeIntPair(UnitType t, int u) : this(bwapiPINVOKE.new_UnitTypeIntPair__SWIG_1(UnitType.getCPtr(t), u), true) {
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public UnitTypeIntPair(UnitTypeIntPair p) : this(bwapiPINVOKE.new_UnitTypeIntPair__SWIG_2(UnitTypeIntPair.getCPtr(p)), true) {
    if (bwapiPINVOKE.SWIGPendingException.Pending) throw bwapiPINVOKE.SWIGPendingException.Retrieve();
  }

  public UnitType first {
    set {
      bwapiPINVOKE.UnitTypeIntPair_first_set(swigCPtr, UnitType.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = bwapiPINVOKE.UnitTypeIntPair_first_get(swigCPtr);
      UnitType ret = (cPtr == global::System.IntPtr.Zero) ? null : new UnitType(cPtr, false);
      return ret;
    } 
  }

  public int second {
    set {
      bwapiPINVOKE.UnitTypeIntPair_second_set(swigCPtr, value);
    } 
    get {
      int ret = bwapiPINVOKE.UnitTypeIntPair_second_get(swigCPtr);
      return ret;
    } 
  }

}

}