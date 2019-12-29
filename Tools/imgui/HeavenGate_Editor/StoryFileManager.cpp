#include "StoryFileManager.h"
#include "HeavenGateEditorUtility.h"
#include "StoryJson.h"
#include "StoryTable.h"
#include "CharacterUtility.h"

#include "StoryJsonContentCompiler.h"

#ifdef _WIN32

#include "Dirent/dirent.h"


#else
#include <dirent.h>


#endif // _WIN32

namespace HeavenGateEditor {


    bool StoryFileManager::LoadStoryFile(const char* pPath, StoryJson* pStoryJson)
{
        std::ifstream fins;
        char content[MAX_FULL_CONTENT];
        memset(content, 0, sizeof(content));

        if (pPath == nullptr || pStoryJson == nullptr)
        {
            return false;
        }

        fins.open(pPath);

        // If it could not open the file then exit.
        if (!fins.fail())
        {
            int i = 0;
            while (!fins.eof())
            {
                fins >> content[i++];
            }

            fins.close();
        }
        else
        {
            std::cerr << "Error: " << strerror(errno);
            return false;
        }

        json a = json::parse(content);


        *pStoryJson = std::move(a);
        pStoryJson->SetFullPath(pPath);

        std::cout << a;
        fins.close();

        return true;
    }

    bool StoryFileManager::SaveStoryFile(const StoryJson* pStoryJson)
    {
        if (pStoryJson == nullptr) {
            return false;
        }
        char filePath[MAX_FOLDER_PATH];
        strcpy( filePath , pStoryJson->GetFullPath());
        
        if (strlen(filePath) <= 0) {
            return false;
        }

        json tmpJson = *pStoryJson;

        std::ofstream o(filePath);
        o << tmpJson << std::endl;

        o.close();

        //Initialize();

        return true;
    }

    bool StoryFileManager::ExportStoryFile(const StoryJson* pStoryJson)
    {
        const char STORY_FOLDER[] = "Storys";
        char tmpPath[MAX_FOLDER_PATH];
        const int folderLength = strlen(STORY_FOLDER);

        if (pStoryJson == nullptr) {
            return false;
        }

        char filePath[MAX_FOLDER_PATH];
        strcpy(filePath, pStoryJson->GetFullPath());

        if (strlen(filePath) <= 0) {
            return false;
        }


        int pos = CharacterUtility::Find(filePath, strlen(filePath), STORY_FOLDER, strlen(STORY_FOLDER));
        if (pos == -1)
        {
            return true;
        }
        int posNeedMove = pos + folderLength;
        strcpy(tmpPath, filePath+posNeedMove);
        strcpy(filePath+pos, STORY_EXPORT_FOLDER);
        strcat(filePath, tmpPath);

        StoryJson copyStory(*pStoryJson);
        StoryJsonContentCompiler::Instance().Compile(&copyStory);
        json tmpJson = copyStory;


        std::ofstream o(filePath);
        o << tmpJson << std::endl;

        o.close();

        //Initialize();

        return true;
    }

    void StoryFileManager::InitFileList(char(*pOutFileList)[MAX_FOLDER_PATH], int* maxFileCount)
    {

        char exePath[MAX_FOLDER_PATH];
        DIR *dir;
        struct dirent *ent;
        int fileCount = 0;


        HeavenGateEditorUtility::GetStoryPath(exePath);
        printf("Current Path:%s", exePath);
    
        if ((dir = opendir(exePath)) != NULL) {

            /* print all the files and directories within directory */
            while ((ent = readdir(dir)) != NULL) {

                printf("%s\n", ent->d_name);
                //Unity meta files
                if (CharacterUtility::Find(
                    ent->d_name,
                    strlen(ent->d_name),
                    ".meta",
                    strlen(".meta")) != -1)
                {
                    continue;
                }

                //MacOS system files
                if (CharacterUtility::Find(
                    ent->d_name,
                    strlen(ent->d_name),
                    ".DS_Store",
                    strlen(".DS_Store")) != -1)
                {
                    continue;
                }

                strcpy(pOutFileList[fileCount], ent->d_name);
                fileCount++;
            }
            closedir(dir);

            *maxFileCount = fileCount;
        }
        else {
            /* could not open directory */
            perror("");
            printf("Can`t open story folder");
            *maxFileCount = -1;
        }

    }

    //bool StoryFileManager::GetIsSaveFileExist()
    //{
    //    return m_isSaveFileExist;
    //}
 
    //char * StoryFileManager::GetNewFileName()
    //{
    //    return const_cast<char*>(const_cast<const StoryFileManager*>(this)->GetNewFileName());
    //}

    //const char * StoryFileManager::GetNewFileName() const
    //{
    //    return m_newFileName;
    //}

    //char * StoryFileManager::GetNewFilePath()
    //{
    //    return const_cast<char *>(const_cast<const StoryFileManager*>(this)->GetNewFilePath());
    //}

    //const char * StoryFileManager::GetNewFilePath() const
    //{
    //    return m_newFilePath;
    //}

 /*   void StoryFileManager::Initialize()
    {
        memset(m_newFilePath, 0, sizeof(m_newFilePath));
        memset(m_newFileName, 0, sizeof(m_newFileName));
    }*/

    //bool StoryFileManager::IsNewFilePathExist() const
    //{
    //    if (strlen(m_newFilePath) > 0) {
    //        return true;
    //    }
    //    else {
    //        return false;
    //    }
    //}

    bool StoryFileManager::FromFileNameToFullPath(char * filePath, const char * fileName) const
    {
        int length = static_cast<int>(strlen(fileName));

        if (length <= 0 || length >= MAX_FILE_NAME) {
            return false;
        }

        memset(filePath, 0, MAX_FOLDER_PATH);

        char storyPath[MAX_FOLDER_PATH];

        HeavenGateEditorUtility::GetStoryPath(storyPath);
        strcat(filePath, storyPath);

#ifdef _WIN32
        strcat(filePath, "\\");
#else
        strcat(filePath, "/");
#endif
        strcat(filePath, fileName);

        strcat(filePath, ".json");

        return true;
    }



    void StoryFileManager::GetFileContent(char* pFullPath, char* pOutContent)
    {
  
      /*  assert(MAX_CONTENT == sizeof(pOutContent));*/

        memset(pOutContent, '\0', MAX_FULL_CONTENT);

        std::ifstream fin;

        fin.open(pFullPath);

        // If it could not open the file then exit.
        if (!fin.fail())
        {
            int i = 0;
            while (!fin.eof() && i < MAX_FULL_CONTENT)
            {
                if (i >= MAX_FULL_CONTENT) {
                    std::cerr << "Out of max content limit";
                }
                fin >> pOutContent[i++];
            }

            fin.close();
        }
    }



    //void StoryFileManager::SetNewFilePath(const char * filePath)
    //{
    //    strcpy(m_newFilePath, filePath);
    //}
 
}
