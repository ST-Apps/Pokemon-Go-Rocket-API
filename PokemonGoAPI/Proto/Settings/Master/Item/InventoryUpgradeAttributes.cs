// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: InventoryUpgradeAttributes.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace POGOProtos.Settings.Master.Item {

  /// <summary>Holder for reflection information generated from InventoryUpgradeAttributes.proto</summary>
  public static partial class InventoryUpgradeAttributesReflection {

    #region Descriptor
    /// <summary>File descriptor for InventoryUpgradeAttributes.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static InventoryUpgradeAttributesReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiBJbnZlbnRvcnlVcGdyYWRlQXR0cmlidXRlcy5wcm90bxIfUE9HT1Byb3Rv",
            "cy5TZXR0aW5ncy5NYXN0ZXIuSXRlbRovUE9HT1Byb3Rvcy9JbnZlbnRvcnkv",
            "SW52ZW50b3J5VXBncmFkZVR5cGUucHJvdG8iegoaSW52ZW50b3J5VXBncmFk",
            "ZUF0dHJpYnV0ZXMSGgoSYWRkaXRpb25hbF9zdG9yYWdlGAEgASgFEkAKDHVw",
            "Z3JhZGVfdHlwZRgCIAEoDjIqLlBPR09Qcm90b3MuSW52ZW50b3J5LkludmVu",
            "dG9yeVVwZ3JhZGVUeXBlYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::POGOProtos.Inventory.InventoryUpgradeTypeReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::POGOProtos.Settings.Master.Item.InventoryUpgradeAttributes), global::POGOProtos.Settings.Master.Item.InventoryUpgradeAttributes.Parser, new[]{ "AdditionalStorage", "UpgradeType" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class InventoryUpgradeAttributes : pb::IMessage<InventoryUpgradeAttributes> {
    private static readonly pb::MessageParser<InventoryUpgradeAttributes> _parser = new pb::MessageParser<InventoryUpgradeAttributes>(() => new InventoryUpgradeAttributes());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<InventoryUpgradeAttributes> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::POGOProtos.Settings.Master.Item.InventoryUpgradeAttributesReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InventoryUpgradeAttributes() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InventoryUpgradeAttributes(InventoryUpgradeAttributes other) : this() {
      additionalStorage_ = other.additionalStorage_;
      upgradeType_ = other.upgradeType_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public InventoryUpgradeAttributes Clone() {
      return new InventoryUpgradeAttributes(this);
    }

    /// <summary>Field number for the "additional_storage" field.</summary>
    public const int AdditionalStorageFieldNumber = 1;
    private int additionalStorage_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int AdditionalStorage {
      get { return additionalStorage_; }
      set {
        additionalStorage_ = value;
      }
    }

    /// <summary>Field number for the "upgrade_type" field.</summary>
    public const int UpgradeTypeFieldNumber = 2;
    private global::POGOProtos.Inventory.InventoryUpgradeType upgradeType_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::POGOProtos.Inventory.InventoryUpgradeType UpgradeType {
      get { return upgradeType_; }
      set {
        upgradeType_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as InventoryUpgradeAttributes);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(InventoryUpgradeAttributes other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (AdditionalStorage != other.AdditionalStorage) return false;
      if (UpgradeType != other.UpgradeType) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (AdditionalStorage != 0) hash ^= AdditionalStorage.GetHashCode();
      if (UpgradeType != 0) hash ^= UpgradeType.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (AdditionalStorage != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(AdditionalStorage);
      }
      if (UpgradeType != 0) {
        output.WriteRawTag(16);
        output.WriteEnum((int) UpgradeType);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (AdditionalStorage != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(AdditionalStorage);
      }
      if (UpgradeType != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) UpgradeType);
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(InventoryUpgradeAttributes other) {
      if (other == null) {
        return;
      }
      if (other.AdditionalStorage != 0) {
        AdditionalStorage = other.AdditionalStorage;
      }
      if (other.UpgradeType != 0) {
        UpgradeType = other.UpgradeType;
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
            AdditionalStorage = input.ReadInt32();
            break;
          }
          case 16: {
            upgradeType_ = (global::POGOProtos.Inventory.InventoryUpgradeType) input.ReadEnum();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
