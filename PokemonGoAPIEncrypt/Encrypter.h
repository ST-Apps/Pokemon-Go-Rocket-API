#pragma once

namespace PokemonGoAPI {
		
	public ref class Encrypter sealed
	{
	private:
		Encrypter();
	public:

		static unsigned int GetOutputSize(const Platform::Array<unsigned char>^ input, const Platform::Array<unsigned char>^ iv);
		static void Encrypt(const Platform::Array<unsigned char>^ input, const Platform::Array<unsigned char>^ iv, Platform::WriteOnlyArray<unsigned char>^ output);
	};
}