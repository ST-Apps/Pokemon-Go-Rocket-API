using Google.Protobuf;
using POGOProtos.Networking.Responses;
using System;

/*
 * Backported from 2.0 and filtered only needed events
 */


namespace PokemonGo.RocketAPI
{
    public partial class Client
    {

        #region Events

		///<summary>
		/// Fires every time a <see cref="CheckAwardedBadgesResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<CheckAwardedBadgesResponse> CheckAwardedBadgesReceived;

		///<summary>
		/// Fires every time a <see cref="CheckChallengeResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<CheckChallengeResponse> CheckChallengeReceived;


		///<summary>
		/// Fires every time a <see cref="DownloadItemTemplatesResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<DownloadItemTemplatesResponse> DownloadItemTemplatesReceived;

		///<summary>
		/// Fires every time a <see cref="DownloadRemoteConfigVersionResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<DownloadRemoteConfigVersionResponse> DownloadRemoteConfigVersionReceived;

		///<summary>
		/// Fires every time a <see cref="DownloadSettingsResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<DownloadSettingsResponse> DownloadSettingsReceived;

		///<summary>
		/// Fires every time a <see cref="GetHatchedEggsResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<GetHatchedEggsResponse> HatchedEggsReceived;


		///<summary>
		/// Fires every time a <see cref="GetInventoryResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<GetInventoryResponse> InventoryReceived;

		///<summary>
		/// Fires every time a <see cref="GetPlayerProfileResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<GetPlayerProfileResponse> PlayerProfileReceived;

		///<summary>
		/// Fires every time a <see cref="GetPlayerResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<GetPlayerResponse> PlayerReceived;

		///<summary>
		/// Fires every time a <see cref="LevelUpRewardsResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<LevelUpRewardsResponse> LevelUpRewardsReceived;

		///<summary>
		/// Fires every time a <see cref="PlayerUpdateResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<PlayerUpdateResponse> PlayerUpdateReceived;

		///<summary>
		/// Fires every time a <see cref="VerifyChallengeResponse" /> is received from the API.
		/// </summary>
		public event EventHandler<VerifyChallengeResponse> VerifyChallengeReceived;


		#endregion

		#region Event Raisers



        /// <summary>
        /// Provides a safe way to invoke the <see cref="CheckAwardedBadgesReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaiseCheckAwardedBadgesReceived(CheckAwardedBadgesResponse value) => CheckAwardedBadgesReceived?.Invoke(this, value);


        /// <summary>
        /// Provides a safe way to invoke the <see cref="CheckChallengeReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaiseCheckChallengeReceived(CheckChallengeResponse value) => CheckChallengeReceived?.Invoke(this, value);



        /// <summary>
        /// Provides a safe way to invoke the <see cref="DownloadItemTemplatesReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaiseDownloadItemTemplatesReceived(DownloadItemTemplatesResponse value) => DownloadItemTemplatesReceived?.Invoke(this, value);


        /// <summary>
        /// Provides a safe way to invoke the <see cref="DownloadRemoteConfigVersionReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaiseDownloadRemoteConfigVersionReceived(DownloadRemoteConfigVersionResponse value) => DownloadRemoteConfigVersionReceived?.Invoke(this, value);


        /// <summary>
        /// Provides a safe way to invoke the <see cref="DownloadSettingsReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaiseDownloadSettingsReceived(DownloadSettingsResponse value) => DownloadSettingsReceived?.Invoke(this, value);


        /// <summary>
        /// Provides a safe way to invoke the <see cref="HatchedEggsReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaiseHatchedEggsReceived(GetHatchedEggsResponse value) => HatchedEggsReceived?.Invoke(this, value);


        /// <summary>
        /// Provides a safe way to invoke the <see cref="InventoryReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaiseInventoryReceived(GetInventoryResponse value) => InventoryReceived?.Invoke(this, value);


        /// <summary>
        /// Provides a safe way to invoke the <see cref="PlayerProfileReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaisePlayerProfileReceived(GetPlayerProfileResponse value) => PlayerProfileReceived?.Invoke(this, value);


        /// <summary>
        /// Provides a safe way to invoke the <see cref="PlayerReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaisePlayerReceived(GetPlayerResponse value) => PlayerReceived?.Invoke(this, value);



        /// <summary>
        /// Provides a safe way to invoke the <see cref="LevelUpRewardsReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaiseLevelUpRewardsReceived(LevelUpRewardsResponse value) => LevelUpRewardsReceived?.Invoke(this, value);


        /// <summary>
        /// Provides a safe way to invoke the <see cref="PlayerUpdateReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaisePlayerUpdateReceived(PlayerUpdateResponse value) => PlayerUpdateReceived?.Invoke(this, value);


        /// <summary>
        /// Provides a safe way to invoke the <see cref="VerifyChallengeReceived" /> event.
        /// </summary>
        /// <param name="value"></param>
        public void RaiseVerifyChallengeReceived(VerifyChallengeResponse value) => VerifyChallengeReceived?.Invoke(this, value);


		#endregion

		#region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        internal bool ProcessMessages(IMessage[] messages)
        {
			var wasSuccessful = true;
            foreach (var message in messages)
            {
				switch (message.GetType().Name)
				{
					case nameof(CheckAwardedBadgesResponse):
						RaiseCheckAwardedBadgesReceived(message as CheckAwardedBadgesResponse);
						break;
					case nameof(CheckChallengeResponse):
						RaiseCheckChallengeReceived(message as CheckChallengeResponse);
						break;
					case nameof(DownloadItemTemplatesResponse):
						RaiseDownloadItemTemplatesReceived(message as DownloadItemTemplatesResponse);
						break;
					case nameof(DownloadRemoteConfigVersionResponse):
						RaiseDownloadRemoteConfigVersionReceived(message as DownloadRemoteConfigVersionResponse);
						break;
					case nameof(DownloadSettingsResponse):
						RaiseDownloadSettingsReceived(message as DownloadSettingsResponse);
						break;
					case nameof(GetInventoryResponse):
						RaiseInventoryReceived(message as GetInventoryResponse);
						break;
					case nameof(GetPlayerProfileResponse):
						RaisePlayerProfileReceived(message as GetPlayerProfileResponse);
						break;
					case nameof(GetPlayerResponse):
						RaisePlayerReceived(message as GetPlayerResponse);
						break;
					case nameof(VerifyChallengeResponse):
						RaiseVerifyChallengeReceived(message as VerifyChallengeResponse);
						break;
					default:
						// @robertmclaws: We got a payload we didn't understand, and couldn't process.
						wasSuccessful = false;
						break;
				}
            }

            return wasSuccessful;
        }

		#endregion

	}

}