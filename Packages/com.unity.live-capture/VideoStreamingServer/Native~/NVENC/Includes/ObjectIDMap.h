#pragma once

#include <unordered_map>

namespace NvencPlugin
{
    // A simple object mapper class that binds object pointers and integer IDs.
    template <typename T> class ObjectIDMap final
    {
    public:
        inline void Add(T* instance)
        {
            static int counter = 1;
            map_[counter++] = instance;
        }

        inline void Remove(T* instance)
        {
            map_.erase(GetID(instance));
        }

        inline T* operator [] (int id) const
        {
            return map_.at(id);
        }

        inline int GetID(T* instance) const
        {
            for (auto it = map_.begin(); it != map_.end(); ++it)
                if (it->second == instance) return it->first;
            return -1;
        }

    private:
        std::unordered_map<int, T*> map_;
    };

    template <typename T> class IDObjectMap final
    {
    public:
        inline void Add(int id, T* instance)
        {
            auto it = map_.find(id);
            if (it != map_.end())
            {
                it->second = instance;
            }
            else
            {
                map_[id] = instance;
            }
        }

        inline void Remove(int id)
        {
            map_.erase(id);
        }

        inline T* operator [] (int id) const
        {
            return map_.at(id);
        }

        inline T* GetInstance(int id) const
        {
            for (auto it = map_.begin(); it != map_.end(); ++it)
                if (it->first == id)
                    return it->second;
            return nullptr;
        }

    private:
        std::unordered_map<int, T*> map_;
    };
}
