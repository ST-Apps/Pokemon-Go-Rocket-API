#ifndef ENCRYPT_H
#define ENCRYPT_H

extern int encrypt(const unsigned char *input, size_t input_size,
	const unsigned char* iv, size_t iv_size,
	unsigned char* output, size_t * output_size);

#endif
