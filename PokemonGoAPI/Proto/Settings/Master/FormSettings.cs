// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: POGOProtos/Settings/Master/FormSettings.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace POGOProtos.Settings.Master {

  /// <summary>Holder for reflection information generated from POGOProtos/Settings/Master/FormSettings.proto</summary>
  public static partial class FormSettingsReflection {

    #region Descriptor
    /// <summary>File descriptor for POGOProtos/Settings/Master/FormSettings.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static FormSettingsReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Ci1QT0dPUHJvdG9zL1NldHRpbmdzL01hc3Rlci9Gb3JtU2V0dGluZ3MucHJv",
            "dG8SGlBPR09Qcm90b3MuU2V0dGluZ3MuTWFzdGVyGhtQT0dPUHJvdG9zL0Vu",
            "dW1zL0Zvcm0ucHJvdG8aIFBPR09Qcm90b3MvRW51bXMvUG9rZW1vbklkLnBy",
            "b3RvIsQBCgxGb3JtU2V0dGluZ3MSLAoHcG9rZW1vbhgBIAEoDjIbLlBPR09Q",
            "cm90b3MuRW51bXMuUG9rZW1vbklkEjwKBWZvcm1zGAIgAygLMi0uUE9HT1By",
            "b3Rvcy5TZXR0aW5ncy5NYXN0ZXIuRm9ybVNldHRpbmdzLkZvcm0aSAoERm9y",
            "bRIkCgRmb3JtGAEgASgOMhYuUE9HT1Byb3Rvcy5FbnVtcy5Gb3JtEhoKEmFz",
            "c2V0X2J1bmRsZV92YWx1ZRgCIAEoBWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::POGOProtos.Enums.FormReflection.Descriptor, global::POGOProtos.Enums.PokemonIdReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::POGOProtos.Settings.Master.FormSettings), global::POGOProtos.Settings.Master.FormSettings.Parser, new[]{ "Pokemon", "Forms" }, null, null, new pbr::GeneratedClrTypeInfo[] { new pbr::GeneratedClrTypeInfo(typeof(global::POGOProtos.Settings.Master.FormSettings.Types.Form), global::POGOProtos.Settings.Master.FormSettings.Types.Form.Parser, new[]{ "Form_", "AssetBundleValue" }, null, null, null)})
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class FormSettings : pb::IMessage<FormSettings> {
    private static readonly pb::MessageParser<FormSettings> _parser = new pb::MessageParser<FormSettings>(() => new FormSettings());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<FormSettings> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::POGOProtos.Settings.Master.FormSettingsReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public FormSettings() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public FormSettings(FormSettings other) : this() {
      pokemon_ = other.pokemon_;
      forms_ = other.forms_.Clone();
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public FormSettings Clone() {
      return new FormSettings(this);
    }

    /// <summary>Field number for the "pokemon" field.</summary>
    public const int PokemonFieldNumber = 1;
    private global::POGOProtos.Enums.PokemonId pokemon_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::POGOProtos.Enums.PokemonId Pokemon {
      get { return pokemon_; }
      set {
        pokemon_ = value;
      }
    }

    /// <summary>Field number for the "forms" field.</summary>
    public const int FormsFieldNumber = 2;
    private static readonly pb::FieldCodec<global::POGOProtos.Settings.Master.FormSettings.Types.Form> _repeated_forms_codec
        = pb::FieldCodec.ForMessage(18, global::POGOProtos.Settings.Master.FormSettings.Types.Form.Parser);
    private readonly pbc::RepeatedField<global::POGOProtos.Settings.Master.FormSettings.Types.Form> forms_ = new pbc::RepeatedField<global::POGOProtos.Settings.Master.FormSettings.Types.Form>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::POGOProtos.Settings.Master.FormSettings.Types.Form> Forms {
      get { return forms_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as FormSettings);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(FormSettings other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Pokemon != other.Pokemon) return false;
      if(!forms_.Equals(other.forms_)) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Pokemon != 0) hash ^= Pokemon.GetHashCode();
      hash ^= forms_.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Pokemon != 0) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Pokemon);
      }
      forms_.WriteTo(output, _repeated_forms_codec);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Pokemon != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Pokemon);
      }
      size += forms_.CalculateSize(_repeated_forms_codec);
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(FormSettings other) {
      if (other == null) {
        return;
      }
      if (other.Pokemon != 0) {
        Pokemon = other.Pokemon;
      }
      forms_.Add(other.forms_);
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
            pokemon_ = (global::POGOProtos.Enums.PokemonId) input.ReadEnum();
            break;
          }
          case 18: {
            forms_.AddEntriesFrom(input, _repeated_forms_codec);
            break;
          }
        }
      }
    }

    #region Nested types
    /// <summary>Container for nested types declared in the FormSettings message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static partial class Types {
      public sealed partial class Form : pb::IMessage<Form> {
        private static readonly pb::MessageParser<Form> _parser = new pb::MessageParser<Form>(() => new Form());
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pb::MessageParser<Form> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::POGOProtos.Settings.Master.FormSettings.Descriptor.NestedTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Form() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Form(Form other) : this() {
          form_ = other.form_;
          assetBundleValue_ = other.assetBundleValue_;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Form Clone() {
          return new Form(this);
        }

        /// <summary>Field number for the "form" field.</summary>
        public const int Form_FieldNumber = 1;
        private global::POGOProtos.Enums.Form form_ = 0;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public global::POGOProtos.Enums.Form Form_ {
          get { return form_; }
          set {
            form_ = value;
          }
        }

        /// <summary>Field number for the "asset_bundle_value" field.</summary>
        public const int AssetBundleValueFieldNumber = 2;
        private int assetBundleValue_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int AssetBundleValue {
          get { return assetBundleValue_; }
          set {
            assetBundleValue_ = value;
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
          return Equals(other as Form);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(Form other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (Form_ != other.Form_) return false;
          if (AssetBundleValue != other.AssetBundleValue) return false;
          return true;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
          int hash = 1;
          if (Form_ != 0) hash ^= Form_.GetHashCode();
          if (AssetBundleValue != 0) hash ^= AssetBundleValue.GetHashCode();
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(pb::CodedOutputStream output) {
          if (Form_ != 0) {
            output.WriteRawTag(8);
            output.WriteEnum((int) Form_);
          }
          if (AssetBundleValue != 0) {
            output.WriteRawTag(16);
            output.WriteInt32(AssetBundleValue);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
          int size = 0;
          if (Form_ != 0) {
            size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Form_);
          }
          if (AssetBundleValue != 0) {
            size += 1 + pb::CodedOutputStream.ComputeInt32Size(AssetBundleValue);
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(Form other) {
          if (other == null) {
            return;
          }
          if (other.Form_ != 0) {
            Form_ = other.Form_;
          }
          if (other.AssetBundleValue != 0) {
            AssetBundleValue = other.AssetBundleValue;
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
                form_ = (global::POGOProtos.Enums.Form) input.ReadEnum();
                break;
              }
              case 16: {
                AssetBundleValue = input.ReadInt32();
                break;
              }
            }
          }
        }

      }

    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
