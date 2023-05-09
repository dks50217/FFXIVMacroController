let vm = new Vue({
    mixins: [mixin],
    data() {
        return {
            loading: false,
            isInit: false,
            isStart: false,
            form: {
                macroList: null,
                repeat : 1
            }, 
            tableColumns: [
                { label: '按鍵', prop: 'key', width: 130, type: 'select', options: 'keyOptions', optionLabel: 'label', optionValue:'value' },
                { label: '類型', prop: 'type', width: 130, type: 'select', options: 'typeOptions', optionLabel: 'label', optionValue: 'value' },
                { label: '執行後暫停', prop: 'sleep', width: 130, type: 'input' },
                { label: '刪除', prop: 'keyName', width: 50, type: 'button', event: (item) => { this.handleRemove(item) } }
            ],
            Options: {
                keyOptions: [],
                typeOptions: []
            },
            infoData: {
                gamePath : ''
            }
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
            _self.isStart = true;
            _self.loading = true;

            console.log(_self.form);

            vm.callAPI("../Start", _self.form).load.then(function (response) {
                console.log('result', response);
                _self.loading = false;
                _self.isStart = false;
            });
        },
        onStop() {
            let _self = this;
            _self.isStart = true;
            _self.loading = false;
            _self.isInit = false;

            vm.callAPI("../Stop").load.then(function (response) {
                console.log('result', response);
                _self.loading = false;
                _self.isInit = true;
            });
        },
        onInit() {
            let _self = this;
            _self.callAPI("../Init").load.then(function (response) {
                console.log('result', response);
                _self.form.macroList = response.macroList;
                _self.Options.keyOptions = response.keyOptions;
                _self.Options.typeOptions = response.typeOptions;
                _self.infoData.gamePath = response.gamePath;
                _self.isInit = true;
            });
        },
        handleAdd() {
            let _self = this;

            let param = {
                "type": 1,
                "key": 49,
                "sleep": 3000,
                "keyName": "D1"
            };

            _self.form.macroList.push(param);
        },
        handleRemove(item) {
            let _self = this;
            _self.form.macroList.splice(item.$index, 1);
        }
    },
    mounted() {
        this.onInit();
    },
    el: "#app"
})