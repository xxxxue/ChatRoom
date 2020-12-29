<template>

  <el-container>
    <el-header>群聊</el-header>
    <el-container>

      <el-container>
        <el-main>

          <div class="chat-content">

            <ul>
              <li v-for="(item,index) in list" :key="index">

                <div v-if="item.isSystemMessage" class="sysmsg">
                  <span>{{ item.content }}</span>
                </div>

                <div v-else>
                  <!-- 对方 -->
                  <div class="word" v-if="!item.isMyMessage">
                    <el-avatar :src="item.headUrl" @mouseenter="enter" @mouseleave="leave"/>

                    <div class="info">
                      <p class="time">{{ item.nickName }} <span v-if="isShowTime">{{ item.time }} </span></p>
                      <div class="info-content">{{ item.content }}</div>
                    </div>


                  </div>
                  <!-- 我的 -->
                  <div class="word-my" v-else>
                    <div class="info">
                      <p class="time"><span v-if="isShowTime">{{ item.time }} </span> {{ item.nickName }} </p>
                      <div class="info-content">{{ item.content }}</div>
                    </div>
                    <el-avatar :src="item.headUrl" @mouseenter="enter" @mouseleave="leave"/>
                  </div>
                </div>

              </li>
            </ul>

          </div>

        </el-main>
        <el-footer>

          <div>

            <el-button @click="toEnd" type="primary" icon="el-icon-download" circle></el-button>
            <el-input type="textarea" :rows="4" v-model="inputContent"/>
            <el-button type="primary" @click="sendMessge">发送消息</el-button>
          </div>

        </el-footer>
      </el-container>

      <el-aside width="200px">

        <div>
          <p>在线列表</p>
          <ul ul class="infinite-list" style="overflow:auto">
            <li v-for="(item,index) in onlineUserList" :key="index" class="infinite-list-item">
              {{ item.nickName }}
            </li>
          </ul>

          <el-input type="text" v-model="userName"/>
          <el-input type="text" v-model="passWord"/>
          <el-button type="primary" @click="login">登录</el-button>

        </div>


      </el-aside>
    </el-container>
  </el-container>

</template>

<script lang="ts">
import {Options, Vue} from 'vue-class-component';
import * as SignalR from '@aspnet/signalr';
import {ElNotification} from 'element-plus';

enum Const {
  Url = "http://localhost:5000/",
  MethodName = "Chat",
  Login = "Login",
  GetOnlineUserList = "GetOnlineUserList",
  ReceiveSystemMessage = "ReceiveSystemMessage",
  ReceiveMessageForElse = "ReceiveMessageForElse",
  SendMessageToElse="SendMessageToElse",
}

interface OnlineUser {
  connectionId: string;
  nickName: string;
  userId: number;
  userName: string;
}


interface Message {
  isMyMessage: boolean;
  isSystemMessage: boolean;
  headUrl: string;
  nickName: string;
  time: string;
  content: string;
  userId: number;
}

@Options({
  props: {
    msg: String // prop demo
  },
})
export default class HelloWorld extends Vue {
  msg!: string // prop demo

  // 输入的消息
  inputContent = ""

  // 消息列表
  list: Array<Message> = [];

  // 是否显示时间
  isShowTime = false;

  // 当前登录的用户
  currentUser: OnlineUser | undefined;

  // 登录用的用户名
  userName = "test1";
  passWord = "123";

  // 在线用户列表
  onlineUserList: Array<OnlineUser> = [];

  // 创建 signalR 对象
  connection = new SignalR.HubConnectionBuilder()
      .withUrl(Const.Url.toString() + Const.MethodName.toString(), {
        skipNegotiation: true,
        transport: SignalR.HttpTransportType.WebSockets, // 通讯方式
      })
      .configureLogging(SignalR.LogLevel.Information) // 日志等级
      .build()

  // 界面加载完毕执行
  mounted() {

    // 接收系统消息 ( 匿名函数内部可使用 this
    this.connection.on(Const.ReceiveSystemMessage.toString(), (data) => {

      const json = {
        isMyMessage: false,
        isSystemMessage: true,
        headUrl: "",
        nickName: data.user.nickName,
        time: data.createdTime,
        content: data.content,
        userId: data.userId,
      };
      this.list.push(json);
    });


    // 接收其他人发的消息
    this.connection.on(Const.ReceiveMessageForElse.toString(), (data) => {

      const json = {
        isMyMessage: false,
        isSystemMessage: false,
        headUrl: "",
        nickName: data.user.nickName,
        time: data.createdTime,
        content: data.content,
        userId: data.userId,
      };
      this.list.push(json);
    })

    // 连接服务器
    this.connection.start();
  }


  // 登陆
  async login() {
    const loginResult = await this.connection.invoke(Const.Login.toString(), this.userName, this.passWord);
    if (loginResult.code) {
      this.currentUser = loginResult.data;

      ElNotification({
        title: '登录成功',
        message: '',
        type: 'success'
      });
      const onlineUserListResult = await this.connection.invoke(Const.GetOnlineUserList.toString());

      console.log("查询在线用户列表返回值 ", onlineUserListResult)
      this.onlineUserList = onlineUserListResult.data;

    }
    console.log("登录返回值  ", loginResult)

  }

  // 发送消息
  sendMessge() {
    if (this.inputContent == "") {
      return;
    }

    if (this.currentUser == undefined) {
      alert("请登录后,再发消息..")
      return;
    }

    this.list.push({
      isSystemMessage: false,
      isMyMessage: true,
      headUrl: "",
      nickName: this.userName,
      time: "2020-12-22 20:46:57",
      content: this.inputContent,
      userId: 0,
    })
    this.scrollToEnd()

    this.connection.invoke(Const.SendMessageToElse.toString(), this.inputContent)
    return;
  }

  /**
   * 滚动到底部
   */
  scrollToEnd() {
    //Dom改变结束后执行回调
    this.$nextTick(() => {
      const container = document.getElementsByClassName('chat-content')[0];
      container.scrollTop = container.scrollHeight;
    })
  }

  // 移到最底部
  toEnd() {
    this.scrollToEnd();
  }

  // 鼠标滑到头像,显示时间
  enter() {
    this.isShowTime = true;
  }
  leave() {
    this.isShowTime = false;
  }

}
</script>


<style scoped lang="scss">

.sysmsg {
  text-align: center;
  margin: 0 auto;
  color: darkgray;
  font-size: 12px;
}

.el-header, .el-footer {
  //background-color: #B3C0D1;
  // color: #333;

  text-align: center;
  line-height: 60px;
}

.el-aside {
  //background-color: #D3DCE6;
  //color: #333;

  //text-align: center;
  //line-height: 200px;
}

.el-main {
  // background-color: #E9EEF3;
  //color: #333;

  //text-align: center;
  //line-height: 160px;
}

body > .el-container {
  margin-bottom: 40px;
}

ul {
  list-style: none;
}


.chat-content {
  width: 90%;
  height: 300px;
  padding: 20px;
  margin: 0 auto;
  overflow-y: scroll;
  //overflow:auto;
  .word {
    display: flex;
    margin-bottom: 20px;

    img {
      width: 40px;
      height: 40px;
      border-radius: 50%;
    }

    .info {
      margin-left: 10px;

      .time {
        font-size: 12px;
        color: rgba(51, 51, 51, 0.8);
        margin: 0;
        height: 20px;
        line-height: 20px;
        margin-top: -5px;
      }

      .info-content {
        padding: 10px;
        font-size: 14px;
        background: yellow;
        position: relative;
        margin-top: 8px;
      }

      //小三角形
      .info-content::before {
        position: absolute;
        left: -8px;
        top: 8px;
        content: '';
        border-right: 10px solid yellow;
        border-top: 8px solid transparent;
        border-bottom: 8px solid transparent;
      }
    }
  }

  .word-my {
    display: flex;
    justify-content: flex-end;
    margin-bottom: 20px;

    img {
      width: 40px;
      height: 40px;
      border-radius: 50%;
    }

    .info {
      width: 90%;
      margin-left: 10px;
      text-align: right;

      .time {
        font-size: 12px;
        color: rgba(51, 51, 51, 0.8);
        margin: 0;
        height: 20px;
        line-height: 20px;
        margin-top: -5px;
        margin-right: 10px;
      }

      .info-content {
        max-width: 70%;
        padding: 10px;
        font-size: 14px;
        float: right;
        margin-right: 10px;
        position: relative;
        margin-top: 8px;
        background: #A3C3F6;
        text-align: left;
      }

      //小三角形
      .info-content::after {
        position: absolute;
        right: -8px;
        top: 8px;
        content: '';
        border-left: 10px solid #A3C3F6;
        border-top: 8px solid transparent;
        border-bottom: 8px solid transparent;
      }
    }
  }
}
</style>
