#include "pch.h"
#include <map>
#include <iostream>
#include <windows.h>

#include "Instruction.h"

using namespace std;

// ���� (δ���ǡ�*��)
DWORD_PTR calculate(DWORD_PTR number1, DWORD_PTR number2, char symbol) {
    switch (symbol)
    {
    case '+':
        return number1 + number2;
    case '-':
        return number1 - number2;
    }
}

// ������ʽ (δ���ǡ�*��)
void ParseFormula(char* op, char** pFormula, char** symbols) {
    int n = 1;
    int opLength = strlen(op);
    for (int i = 0; i < opLength; i++) {
        switch (op[i])
        {
        case '+':
            pFormula[n] = symbols[0];
            *(op + i) = '\0';
            n += 2;
            break;
        case '-':
            pFormula[n] = symbols[1];
            *(op + i) = '\0';
            n += 2;
            break;
        }
    }
    n = 0;
    for (int i = 0; i < opLength; i += strlen(op + i) + 1) {
        pFormula[n] = op + i;
        n += 2;
    }
}

// ��ȡ������ֵ�� ����(r�Ĵ���/m�ڴ�ռ�) + ��ַ
DWORD_PTR GetOpTypeAndAddr(char* op, char* pOpType1, PDWORD_PTR pVtRegs, PDWORD_PTR opNumber) {
    char* endPtr;
    char tempOp[50] = "";
    char* symbols[] = { (char*)"+", (char*)"-" };
    char* formula[] = { NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL };

    // ������
    if (op[0] == 'i') {
        *opNumber = strtol(op + 1, &endPtr, 16);
        return (DWORD_PTR)opNumber;
    }
    // lea [] / ptr []
    else if (op[0] == 'l' || op[0] == 'p') {
        // ���������� (������ʽ���̻ᵼ�±仯)
        strcpy_s(tempOp, sizeof(tempOp), op);
        // ������ʽ (δ���ǡ�*��)
        ParseFormula(op + 1, formula, symbols);
        // ���� (δ���ǡ�*��)
        DWORD_PTR number1 = 0;
        DWORD_PTR number2;
        char symbol = '+';
        for (int i = 0; formula[i] != NULL; i++) {
            switch (formula[i][0])
            {
            case 'i':
                DWORD_PTR tempNumber; // �洢����
                GetOpTypeAndAddr(formula[i], NULL, pVtRegs, &tempNumber);
                number1 = calculate(number1, tempNumber, symbol);
                break;
            case 'q':
                number2 = *(PDWORD64)GetOpTypeAndAddr(formula[i], NULL, pVtRegs, NULL);
                number1 = calculate(number1, number2, symbol);
                break;
            case 'd':
                number2 = *(PDWORD)GetOpTypeAndAddr(formula[i], NULL, pVtRegs, NULL);
                number1 = calculate(number1, number2, symbol);
                break;
            case 'w':
                number2 = *(PWORD)GetOpTypeAndAddr(formula[i], NULL, pVtRegs, NULL);
                number1 = calculate(number1, number2, symbol);
                break;
            case 'b':
                number2 = *(PBYTE)GetOpTypeAndAddr(formula[i], NULL, pVtRegs, NULL);
                number1 = calculate(number1, number2, symbol);
                break;
            case '+':
                symbol = '+';
                break;
            case '-':
                symbol = '-';
                break;
            }
        }
        // ��ԭ������
        strcpy_s(op, sizeof(tempOp), tempOp);
        // lea []
        if (op[0] == 'l') {
            *opNumber = number1;
            return (DWORD_PTR)opNumber;
        }
        // ptr []
        if (pOpType1 != NULL) {
            *pOpType1 = 'm';
        }
        return number1;
    }
    // �Ĵ���
    else {
        if (pOpType1 != NULL) {
            *pOpType1 = 'r';
        }
        return (DWORD_PTR)pVtRegs + strtol(op + 1, &endPtr, 16);
    }
}

struct SelfAsm {
    int mnemonicIndex;
    char* opBit1;
    char* op1;
    char* opBit2;
    char* op2;
};

// �����Զ�����
void ParseSelfAsm(char* selfAsm, PDWORD_PTR pVtRegs) {
    int selfAsmLength = strlen(selfAsm) + 1;
    for (int i = 0; i < selfAsmLength; i++) {
        if (*(selfAsm + i) == '_') {
            *(selfAsm + i) = '\0';
        }
    }

    // ��������
    int i = 0;
    int num = 0;
    char* endPtr;
    map<int, DWORD_PTR> vtAddrMapping; // num -> �����ַ
    map<DWORD_PTR, int> numMapping; // �����ַ -> num
    map<DWORD_PTR, SelfAsm> selfAsmMap; // �����ַ -> �Զ�����
    while (selfAsm[i] != '!') {
        // �����ַ
        DWORD_PTR vtAddr = strtol(selfAsm + i, &endPtr, 16);
        i += strlen(selfAsm + i) + 1;
        vtAddrMapping[num] = vtAddr;
        numMapping[vtAddr] = num;

        SelfAsm currentSelfAsm;

        // ���Ƿ����
        currentSelfAsm.mnemonicIndex = atoi(selfAsm + i);
        i += strlen(selfAsm + i) + 1;

        // ������1 λ��
        currentSelfAsm.opBit1 = selfAsm + i;
        i += strlen(selfAsm + i) + 1;

        // ������1
        currentSelfAsm.op1 = selfAsm + i;
        i += strlen(selfAsm + i) + 1;

        // ������2 λ��
        currentSelfAsm.opBit2 = selfAsm + i;
        i += strlen(selfAsm + i) + 1;

        // ������1
        currentSelfAsm.op2 = selfAsm + i;
        i += strlen(selfAsm + i) + 1;


        selfAsmMap[vtAddr] = currentSelfAsm;
        num++;
    }

    // ����ִ��
    for (int i = 0; i < num; i++) {
        pVtRegs[16] = vtAddrMapping[i];
        int mnemonicIndex = selfAsmMap[pVtRegs[16]].mnemonicIndex;
        char* opBit1 = selfAsmMap[pVtRegs[16]].opBit1;
        char* op1 = selfAsmMap[pVtRegs[16]].op1;
        char* opBit2 = selfAsmMap[pVtRegs[16]].opBit2;
        char* op2 = selfAsmMap[pVtRegs[16]].op2;

        // ��ȡ������������ ����(r�Ĵ���/m�ڴ�ռ�) + ��ַ
        char opType1;
        DWORD_PTR opAddr1 = NULL;
        DWORD_PTR opAddr2 = NULL;
        DWORD_PTR opNumber; // �洢����
        if (strlen(op1)) {
            opAddr1 = GetOpTypeAndAddr(op1, &opType1, pVtRegs, &opNumber);
        }
        if (strlen(op2)) {
            opAddr2 = GetOpTypeAndAddr(op2, NULL, pVtRegs, &opNumber);
        }

        // ����ָ��
        if (InvokeInstruction(mnemonicIndex, opType1, opBit1[0], opAddr1, opBit2[0], opAddr2, pVtRegs)) {
            i = numMapping[pVtRegs[16]]; // Jcc ָ����ת
            i--;
        }
    }
}

// ħ������
void MagicInvoke(char* selfAsm, char* commandPara, int commandParaLength, char** pOutputData, int* pOutputDataLength, PVOID* pFuncAddr) {
    // ��������ջ
    PVOID vtStack = malloc(0x10000);

    // ��������Ĵ���
    /*
    * 0  RAX
    * 1  RBX
    * 2  RCX
    * 3  RDX
    * 4  RSI
    * 5  RDI
    * 6  R8
    * 7  R9
    * 8  R10
    * 9  R11
    * 10 R12
    * 11 R13
    * 12 R14
    * 13 R15
    * 14 RSP
    * 15 RBP
    * 16 RIP
    * 17 EFL
    */
    DWORD_PTR vtRegs[18] = { 0 };
    vtRegs[14] = vtRegs[15] = (DWORD_PTR)vtStack + 0x9000;

    // ��������Ĵ����ĳ�ֵ
    /*
    * ShellCode(commandPara, commandParaLength, &outputData, &outputDataLength, funcAddr);
    * lea rax, [funcAddr]
    * mov qword ptr [rsp+20h], rax
    * lea r9, [outputDataLength]
    * lea r8, [outputData]
    * mov edx, dword ptr [commandParaLength]
    * lea rcx, [commandPara]
    * call ShellCode
    */
    vtRegs[0] = (DWORD_PTR)pFuncAddr;
    *(PDWORD_PTR)(vtRegs[14] + 0x20) = vtRegs[0];
    vtRegs[7] = (DWORD_PTR)pOutputDataLength;
    vtRegs[6] = (DWORD_PTR)pOutputData;
    vtRegs[3] = commandParaLength;
    vtRegs[2] = (DWORD_PTR)commandPara;
    vtRegs[14] = vtRegs[14] - sizeof(DWORD_PTR);

    // �����Զ�����
    ParseSelfAsm(selfAsm, vtRegs);
    free(vtStack);
}