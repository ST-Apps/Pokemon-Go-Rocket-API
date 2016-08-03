// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: POGOProtos/Networking/Responses/EquipBadgeResponse.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace POGOProtos.Networking.Responses {

  /// <summary>Holder for reflection information generated from POGOProtos/Networking/Responses/EquipBadgeResponse.proto</summary>
  public static partial class EquipBadgeResponseReflection {

    #region Descriptor
    /// <summary>File descriptor for POGOProtos/Networking/Responses/EquipBadgeResponse.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static EquipBadgeResponseReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjhQT0dPUHJvdG9zL05ldHdvcmtpbmcvUmVzcG9uc2VzL0VxdWlwQmFkZ2VS",
            "ZXNwb25zZS5wcm90bxIfUE9HT1Byb3Rvcy5OZXR3b3JraW5nLlJlc3BvbnNl",
            "cxoqUE9HT1Byb3Rvcy9EYXRhL1BsYXllci9FcXVpcHBlZEJhZGdlLnByb3Rv",
            "IuMBChJFcXVpcEJhZGdlUmVzcG9uc2USSgoGcmVzdWx0GAEgASgOMjouUE9H",
            "T1Byb3Rvcy5OZXR3b3JraW5nLlJlc3BvbnNlcy5FcXVpcEJhZGdlUmVzcG9u",
            "c2UuUmVzdWx0EjcKCGVxdWlwcGVkGAIgASgLMiUuUE9HT1Byb3Rvcy5EYXRh",
            "LlBsYXllci5FcXVpcHBlZEJhZGdlIkgKBlJlc3VsdBIJCgVVTlNFVBAAEgsK",
            "B1NVQ0NFU1MQARITCg9DT09MRE9XTl9BQ1RJVkUQAhIRCg1OT1RfUVVBTElG",
            "SUVEEANiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::POGOProtos.Data.Player.EquippedBadgeReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::POGOProtos.Networking.Responses.EquipBadgeResponse), global::POGOProtos.Networking.Responses.EquipBadgeResponse.Parser, new[]{ "Result", "Equipped" }, null, new[]{ typeof(global::POGOProtos.Networking.Responses.EquipBadgeResponse.Types.Result) }, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class EquipBadgeResponse : pb::IMessage<EquipBadgeResponse> {
    private static readonly pb::MessageParser<EquipBadgeResponse> _parser = new pb::MessageParser<EquipBadgeResponse>(() => new EquipBadgeResponse());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<EquipBadgeResponse> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::POGOProtos.Networking.Responses.EquipBadgeResponseReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public EquipBadgeResponse() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public EquipBadgeResponse(EquipBadgeResponse other) : this() {
      result_ = other.result_;
      Equipped = other.equipped_ != null ? other.Equipped.Clone() : null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public EquipBadgeResponse Clone() {
      return new EquipBadgeResponse(this);
    }

    /// <summary>Field number for the "result" field.</summary>
    public const int ResultFieldNumber = 1;
    private global::POGOProtos.Networking.Responses.EquipBadgeResponse.Types.Result result_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::POGOProtos.Networking.Responses.EquipBadgeResponse.Types.Result Result {
      get { return result_; }
      set {
        result_ = value;
      }
    }

    /// <summary>Field number for the "equipped" field.</summary>
    public const int EquippedFieldNumber = 2;
    private global::POGOProtos.Data.Player.EquippedBadge equipped_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::POGOProtos.Data.Player.EquippedBadge Equipped {
      get { return equipped_; }
      set {
        equipped_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as EquipBadgeResponse);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(EquipBadgeResponse other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Result != other.Result) return false;
      if (!object.Equals(Equipped, other.Equipped)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Result != 0) hash ^= Result.GetHashCode();
      if (equipped_ != null) hash ^= Equipped.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Result != 0) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Result);
      }
      if (equipped_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(Equipped);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Result != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Result);
      }
      if (equipped_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Equipped);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(EquipBadgeResponse other) {
      if (other == null) {
        return;
      }
      if (other.Result != 0) {
        Result = other.Result;
      }
      if (other.equipped_ != null) {
        if (equipped_ == null) {
          equipped_ = new global::POGOProtos.Data.Player.EquippedBadge();
        }
        Equipped.MergeFrom(other.Equipped);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            result_ = (global::POGOProtos.Networking.Responses.EquipBadgeResponse.Types.Result) input.ReadEnum();
            break;
          }
          case 18: {
            if (equipped_ == null) {
              equipped_ = new global::POGOProtos.Data.Player.EquippedBadge();
            }
            input.ReadMessage(equipped_);
            break;
          }
        }
      }
    }

    #region Nested types
    /// <summary>Container for nested types declared in the EquipBadgeResponse message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static partial class Types {
      public enum Result {
        [pbr::OriginalName("UNSET")] Unset = 0,
        [pbr::OriginalName("SUCCESS")] Success = 1,
        [pbr::OriginalName("COOLDOWN_ACTIVE")] CooldownActive = 2,
        [pbr::OriginalName("NOT_QUALIFIED")] NotQualified = 3,
      }

    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
