//
//Saint Engine Character Utility
//

#ifndef CHARACTER_UTILITY_H
#define CHARACTER_UTILITY_H

#include <string>


class CharacterUtility
{
public:
	static int BUFFERSIZE;

	//static int getLength(const char* c);
	static void reverse(char* c);
	static void parseSuffix(const char* c, char* suffix);

	static void wreverse(wchar_t* c);
	static void wparseSuffix(const wchar_t* c, wchar_t* suffix);

    static size_t convertWcsToMbs(char* pDest,const wchar_t* pSrc, int bufferSize = BUFFERSIZE);
    static size_t convertMbsToWcs(wchar_t* pDest, const char* pSrc, int bufferSize = BUFFERSIZE);

    static int Find(const char* pContent, int contentLegth, const char* pFind, int findLength);

	static bool copyCharPointer(char* pDest, const char* pSrc);
	static bool copyWcharPointer(wchar_t* pDest, const wchar_t* pSrc);

private:

    class DFA
    {
    public:
        int* m_next;
        char* m_character;
        int m_characterCount;
        int m_countentlength;


        int Get(int characterPos, int pos);

        void Set(int characterPos, int pos, int value);
        char GetCharacter(int pos);
        int GetCharacterPos(char character);

        DFA();
        ~DFA();

    private:

    };
	static void readSuffix(const char* c, char* suffix); 
	static void wreadSuffix(const wchar_t* c, wchar_t* suffix);

    static void CreateDFA(const char* findContent,int findLength, DFA* pOutDFA );
	CharacterUtility();
	~CharacterUtility();
};


#endif // !CHARACTER_UTILITY_H
