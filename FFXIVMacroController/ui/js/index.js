let vm = new Vue({
    mixins: [mixin],
    data() {
        return {
            form: {
               
            },
            loading : false,
        }
    },
    components: {
       
    },
    created() {

    },
    computed: {

    },
    methods: {
        onSubmit(isStart){
            let _self = this;
            let router = isStart ? "../Start" : "../Stop"
            _self.loading = true;
            vm.callAPI(router, this.form).load.then(function (response) {
                console.log('result', response);
            });
        }
    },
    mounted() {
       
    },
    el: "#app"
})