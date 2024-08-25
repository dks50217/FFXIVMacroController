let vm = new Vue({
    mixins: [mixin],
    data() {
        return {
            loading: false,
            isInit: false,
            isStart: false,
            saveLoader: false,
            fullscreenLoading: false,
            isEditTitle: false,
            form: {
                macroList: null,
                rootData : null,
                repeat: 1,
                dialogFormVisible: false,
                formLabelWidth: '120px',
                isInsert: false,
                insertMacro: null,
                insertNumber: 1
            }, 
            tableColumns: [
                { label: '按鍵', prop: 'keyNumber', width: 230, type: 'select', options: 'keyOptions', optionLabel: 'label', optionValue: 'value' },
                //{ label: '定位', prop: 'locate', btnType: 'primary', icon: 'el-icon-position', width: 50, type: 'label', type: 'button', event: (item, scope) => { this.handleLocate(item, scope) } },
                //{ label: '座標', prop: 'coordinate', width: 130, type: 'label', width: 100 },
                { label: '類型', prop: 'type', width: 130, type: 'select', options: 'typeOptions', optionLabel: 'label', optionValue: 'value' },
                { label: '執行後暫停', prop: 'sleep', width: 130, type: 'input' },
                { label: '文字', prop: 'inputText', width: 300, type: 'textarea' },
                { label: '刪除', prop: 'keyName', fixed: "right", btnType: 'danger', icon: 'el-icon-delete', width: 50, type: 'button', event: (item, scope) => { this.handleRemove(item, scope) } },
               /* { label: '排序', prop: 'key', btnType: 'success', type: 'button', icon: 'el-icon-rank', width: 50, class: 'drag', event: () => void(0)}*/
            ],
            Options: {
                keyOptions: [],
                typeOptions: []
            },
            infoData: {
                gamePath : ''
            },
            dialogData: {
                name: '',
                cloneid: ''
            },
            simulate: {
                jobList: null,
                totalJobs: null,
                completedJobs: 0,
                percentage: 0,
                stop: false,
                timeoutId : null
            },
            connection: null,
            chatMessage: ''
        }
    },
    components: {
       'el-table-draggable': httpVueLoader('../component/el-table-draggable.vue'),
       'raw-displayer': httpVueLoader('../component/raw-displayer.vue')
    },
    created() {

    },
    computed: {

    },
    methods: {
        onStart(){
            let _self = this;
            
            let param = _self.form.rootData.categoryList.find(c => c.id == _self.form.rootData.rootID);

            let apiObj = Object.assign({}, param);

            if (_self.form.isInsert) {

                if (!_self.form.insertMacro) {
                    _self.$message({
                        showClose: true,
                        message: '穿插要設定穿插的腳本',
                        type: 'error'
                    });
                    return;
                }

                let insertArray = _self.form.rootData.categoryList.find(c => c.id == _self.form.insertMacro).macroList;
                let insertNumber = _self.form.insertNumber + 1;

                if (insertNumber > _self.form.repeat) {
                    _self.$message({
                        showClose: true,
                        message: '穿插不允許大於或相同於重複次數',
                        type: 'error'
                    });
                    return;
                }

                apiObj.macroList = _self.repeatArrayWithInsertion(param.macroList, _self.form.repeat, insertArray, insertNumber);
            }
            else {
                apiObj.macroList = _self.repeat(param.macroList, _self.form.repeat);
            }
                
            console.log('apiObj', apiObj);

            _self.isStart = true;
            _self.loading = true;
            _self.initSimulater(apiObj, true);

            vm.callAPI("../Start", apiObj).load.then(function (response) {
                console.log('result', response);
                _self.loading = false;
                _self.isStart = false;
                _self.initSimulater(null, false);
            });
        },
        repeat(array, numberOfTimes) {
            return Array(numberOfTimes)
                .fill(array)
                .reduce((a, b) => [...a, ...b], []);
        },
        repeatArrayWithInsertion(array, numberOfTimes, arrayToInsert, insertAtRepeat) {
            return Array(numberOfTimes)
                .fill(array)
                .reduce((a, b, index) => {
                    if (index === insertAtRepeat - 1) {
                        a = a.concat(arrayToInsert);
                    }
                    return a.concat(b);
                }, []);
        },
        initSimulater(apiObj, isStart) {
            let _self = this;

            if (isStart) {
                _self.simulate.jobList = apiObj.macroList;
                _self.simulate.totalJobs = apiObj.macroList.length;
                _self.simulate.completedJobs = 0;
                _self.simulate.percentage = 0;
                _self.simulate.stop = false;
                _self.simulateJobCompletion();
            }
            else {
                _self.simulate.jobList = null;
                _self.simulate.totalJobs = 0;
                _self.simulate.completedJobs = 0;
                _self.simulate.percentage = 0;
                _self.simulate.stop = true;
                clearTimeout(_self.simulate.timeoutId);
            }
        },
        simulateJobCompletion() {
            let _self = this;
            if (_self.simulate.completedJobs < _self.simulate.totalJobs) {
                const job = _self.simulate.jobList[_self.simulate.completedJobs];
                _self.simulate.timeoutId = setTimeout(() => {
                    _self.simulate.completedJobs++;
                    _self.simulate.percentage = Math.round((_self.simulate.completedJobs / _self.simulate.totalJobs) * 100);
                    _self.simulateJobCompletion();
                }, job.sleep * 1000);
            }
        },
        onStop() {
            let _self = this;

            _self.initSimulater(null, false);

            vm.callAPI("../Stop").load.then(function (response) {
                console.log('result Stop', response);
            });
        },
        onInit() {
            let _self = this;
            _self.callAPI("../Init").load.then(function (response) {
                console.log('result', response);
                _self.Options.keyOptions = response.keyOptions;
                _self.Options.typeOptions = response.typeOptions;
                _self.infoData.gamePath = response.gamePath;
                _self.form.rootData = response.rootData;
                _self.isInit = true;

                let messageType = response.gamePath ? 'success' : 'error';
                let messageShow = response.gamePath ? '初始化成功' : '初始化失敗';

                _self.$message({
                    showClose: true,
                    message: messageShow,
                    type: messageType
                });
            });
        },
        handleTabClick() {
            console.log('handleTabClick');
        },
        handleAdd(macroList) {
            let _self = this;
            let param = {};

            if (macroList.length == 0) {
                param = {
                    "type": 1,
                    "key": 49,
                    "sleep": 3,
                    "keyName": "D1",
                    "keyNumber": 49,
                    "coordinateX": 0,
                    "coordinateY": 0,
                    "coordinate": "(0,0)",
                    "inputText": "/echo 123"
                };
            }
            else {
                param = Object.assign({}, macroList[macroList.length - 1]);
            }

            macroList.push(param);
        },
        handleRemove(item, scope) {
            let _self = this;
            item.macroList.splice(scope.$index, 1);
        },
        addCategory() {
            let _self = this;
            _self.form.dialogFormVisible = true;
            _self.dialogData.name = "";
            _self.dialogData.cloneid = "";
        },
        handleLocate(item, scope) {
            let _self = this;

            const loading = _self.$loading({
                lock: true,
                text: '請到對應視窗按下左Ctrl定位滑鼠',
                spinner: 'el-icon-coordinate',
                background: 'rgba(0, 0, 0, 0.7)'
            });

            _self.callAPI("../LocateMouse").load.then(function (response) {
                scope.row.coordinateX = response.coordinateX;
                scope.row.coordinateY = response.coordinateY;
                scope.row.coordinate = "(" + scope.row.coordinateX + "," + scope.row.coordinateY + ")";
                loading.close();

                _self.$message({
                    showClose: true,
                    message: "定位成功 " + scope.row.coordinate, 
                    type: "success"
                });
            });
        },
        onSave() {
            let _self = this;
            _self.saveLoader = true;

            console.log('save',_self.form.rootData);


            _self.callAPI("../Save", _self.form.rootData).load.then(function (response) {
                _self.saveLoader = false;

                if (response) {
                    _self.$message({ message: '儲存成功', type: 'success' });
                }
                else {
                    _self.$message.error('儲存失敗');
                }
            });
        },
        onDialogClose() {
            let _self = this;
            _self.form.dialogFormVisible = false
            let refItem = _self.form.rootData.categoryList.find(c => c.id == _self.dialogData.cloneid);
            const cloneItem = Object.assign({}, refItem);
            cloneItem.id = _self.genUUID();
            cloneItem.name = _self.dialogData.name;
            _self.form.rootData.categoryList.push(cloneItem);
        },
        genUUID() {
            return Array
                .from(Array(16))
                .map(e => Math.floor(Math.random() * 255)
                    .toString(16)
                    .padStart(2, "0"))
                .join('')
                .match(/.{1,4}/g)
                .join('-');
        },
        onInitSignalR() {
            let _self = this;

            _self.connection = new signalR.HubConnectionBuilder()
                .withUrl("../chatHub")
                .build();

            _self.connection.on("ReceiveMessage", (user, message) => {
                _self.chatMessage += `${user}: ${message}` + '\r\n';
            });

            _self.connection.start().catch(err => console.error(err.toString()));
        }
    },
    watch: {
        chatMessage(newVal, oldVal) {
            const textarea = this.$refs.textareaRef.$refs.textarea;
            textarea.scrollTop = textarea.scrollHeight + 100;
        }
    },
    mounted() {
        this.onInit();
        this.onInitSignalR();
    },
    el: "#app"
})