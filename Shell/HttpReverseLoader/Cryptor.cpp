#include <iostream>

using namespace std;

// ��������
void EncryptData(char* data, int dataLength) {
	for (int i = 0; i < dataLength - 1; i++) {
		*(data + i) ^= *(data + dataLength - 1);
	}
}

// ��������
void DecryptData(char* data, int dataLength) {
	for (int i = 0; i < dataLength - 1; i++) {
		*(data + i) ^= *(data + dataLength - 1);
	}
}