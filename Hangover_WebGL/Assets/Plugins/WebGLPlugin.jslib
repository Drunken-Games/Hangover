mergeInto(LibraryManager.library,{
    ClearBrowswerCache: function(){
        if('caches' in window){
            caches.keys().then(cacheNames=>{
                cacheNames.forEach(cacheName=>{
                    caches.delete(cacheName).then(success=>{
                        if(success) console.log(`Cache ${cacheName} deleted.`);
                    })
                })
            })
        }
    }
})