#include "pch.h"
#include "Encrypter.h"

extern "C"
{
#include "encrypt.h"
}

using namespace PokemonGoAPI;
using namespace Platform;

Encrypter::Encrypter()
{
}


unsigned int Encrypter::GetOutputSize(const Array<unsigned char>^ input, const Array<unsigned char>^ iv)
{
	unsigned int outsize = 0;

	encrypt(input->Data, input->Length,
		iv->Data, iv->Length,
		nullptr, &outsize);

	return outsize;
}

void Encrypter::Encrypt(const Array<unsigned char>^ input, const Array<unsigned char>^ iv, WriteOnlyArray<unsigned char>^ output)
{
	unsigned int outsize = output->Length;
	encrypt(input->Data, input->Length,
		iv->Data, iv->Length,
		output->Data, &outsize);
}
