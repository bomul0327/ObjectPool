using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

[Serializable]
public struct PoolObject{
    public GameObject poolPrefab;
    public int poolAmount;
}
public class ObjectPool : MonoBehaviour {
    
    //Singleton
    private static ObjectPool instance;
    public static ObjectPool Instance{
        get{
            return instance;
        }

        set{
            instance = value;
        }
    }

    //Inspector에서 pooling을 할 오브젝트를 기록할 리스트
    public List<PoolObject> poolList;

    //게임 내부에서 오브젝트들을 담아놓을 리스트
    private List<List<GameObject>> poolGameObjectList;
    
    void Awake()
    {
        //Singleton 초기화
        if(instance == null){
            instance = this;
        }
        else{
            Destroy(this.gameObject);
        }

        //poolGameObjectList 초기화
        poolGameObjectList = new List<List<GameObject>>();
        foreach(var pList in poolList){
            poolGameObjectList.Add(new List<GameObject>());
        }

        //처음 사용할 오브젝트들 Instantiate
        for(int i = 0; i < poolList.Count; ++i){
            for(int j = 0; j < poolList[i].poolAmount; ++j){
                GameObject initObject = Instantiate(poolList[i].poolPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                initObject.SetActive(false);
                initObject.transform.SetParent(transform);
                poolGameObjectList[i].Add(initObject);
            }
        }
    }

    //풀에 있는 오브젝트 활성화.
    //만약 풀에 오브젝트가 없으면 새로 생성하고 풀에 추가
    public GameObject CreateObject(int index, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion)){
        
        var instObject = 
        (from iObj in poolGameObjectList[index]
        where iObj.activeInHierarchy == false
        select iObj).FirstOrDefault();

        if(instObject != null){
            instObject.SetActive(true);
            instObject.transform.position = position;
            instObject.transform.rotation = rotation;
        }
        else{
            GameObject prefab = poolList[index].poolPrefab;
            instObject = Instantiate(prefab, position, rotation);
            poolGameObjectList[index].Add(instObject);
        }

        return instObject;
    }

    //풀에 있는 오브젝트 비활성화
    //만약 풀에 오브젝트가 없으면 Destroy() 호출
    public void DestroyObject(GameObject wantToDestroy){
        var destObject = 
        (from dList in poolGameObjectList
        from dObj in dList
        where dObj == wantToDestroy
        select dObj).FirstOrDefault();

        if(destObject != null){
            destObject.SetActive(false);
        }
        else{
            Destroy(destObject);
        }
    }

    //사용하지 않는 리스트 삭제
    public void Dispose(int index){
        if(poolGameObjectList[index] == null) return;

        foreach(var item in poolGameObjectList[index]){
            Destroy(item);
        }
        
        poolGameObjectList[index] = null;
    }

    //오브젝트 풀 삭제
    public void DisposeAll(){
        if(poolGameObjectList == null) return;

        for(int i = 0; i < poolGameObjectList.Count(); ++i){
            Dispose(i);
        }

        poolGameObjectList = null;
    }
}
