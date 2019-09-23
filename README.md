---
layout: post
title: 记一次修改unity3d富文本插件的设计缺陷
key: 201909182243
tags: 插件 富文本
---

[TOC]

小弟最近开发客户端的聊天系统。策划案子要求支持图片和文字混排。我在github上找到了功能较为丰富，start数比较多的插件[TextInlineSprite](https://github.com/coding2233/TextInlineSprite)

它支持：
* 文本超链接显示
* 文本下划线效果
* 支持同时多个表情集

插件提供了详细的样例代码，让你非常容易入手。在此感觉作者[coding2233](https://github.com/coding2233)的贡献


![](https://raw.githubusercontent.com/lizijie/lizijie.github.io/master/assets/images/2019-09-18-%E8%AE%B0%E4%B8%80%E6%AC%A1%E4%BF%AE%E6%94%B9unity3d%E5%AF%8C%E6%96%87%E6%9C%AC%E6%8F%92%E4%BB%B6%E7%9A%84%E8%AE%BE%E8%AE%A1%E7%BC%BA%E9%99%B7/inlinetext_bug.png)

然而，当我滑动无限复用列表时，表情位置显示不正确（如上图）！！特别是在UI项超出边界，被回收复用时，偏移尤为明显。我参考插件样例ChatTest相关代码（如下），当ScrollRect的位置变化时，同时也调整SpriteGraphic的位置，目的是抵消表情位置偏差。不过问题依然没有改善。

```c#
namespace EmojiText.Taurus
{
    
	public class ChatTest : MonoBehaviour
	{
        //滚动文本
		[SerializeField]
		private ScrollRect _scrollView;

        [SerializeField]
		private RectTransform _spriteRect;

        // something here...

        private IEnumerator Start()
		{
			_scrollView.onValueChanged.AddListener(OnSrcollViewChanged);

            // something here...
        }

        private void OnSrcollViewChanged(Vector2 pos)
		{
			_spriteRect.anchoredPosition = _scrollView.content.anchoredPosition;

            // something here...
        }

        // something here...
    }
}
```

经过长时间的调试，我发现在UI复用的一帧非常容易出现位置错误。实际是一个设计缺陷，导致了该问题。我向作者已提了[issues](https://github.com/coding2233/TextInlineSprite/issues/46)

>渲染一个表情在v3.0的实现里，流程大致如下：
>
>**【流程一】** InlineText.OnPopulateMesh转换表情顶点至全局坐标，InlineManager.UpdateTextInfo再将全局坐标转换到SpriteGraphic下的局部坐标。
>
>**【流程二】** InlineManager.Update合批相同表情ID的Mesh，转输到SpriteGraphic.OnPopulateMesh作最终渲染。
>
>因为InlineManager.Update合批Mesh，所以这两个流程相差1帧。如果InlineText的坐标在这帧变动了（如列表划动），因为没其它逻辑去强刷缓存在InlineManager的EmojiText.Taurus.MeshInfo坐标，所以SpriteGraphic.OnPopulateMesh使用的是不正确的坐标。结果显示上表情位置偏移了。
>
>像ChatTest样例中，监听ScrollRect.onValueChanged事件，修复SpriteGraphic坐标并未解决以上问题。因为它是与 **【流程一】**  同帧进行。依然与 **【流程二】** 相差了1帧。
>
>```c#
>private void OnSrcollViewChanged(Vector2 pos)
>{
>    _spriteRect.anchoredPosition = _scrollView.content.anchoredPosition;
>}
>```
>
>**测试代码如下：**
>```c#
>public class Test : MonoBehaviour
>{
>    public InlineText inText = null;
>    public SpriteGraphic graphic = null;
>
>    void Start()
>    {
>        inText.text = "NewText[#emoji_0]";
>
>		// 模拟在【流程一】与【流程二】中间1帧，修改了InlineText坐标
>        graphic.RegisterDirtyVerticesCallback(() =>
>        {
>            inText.transform.localPosition = new Vector3(200, 200, 0);
>        });
>    }
>}
>```
>**执行结果如下**
>![image](https://user-images.githubusercontent.com/3928231/64693604-32aa1380-d4ca-11e9-83c3-c3b6048ba8b4.png)

为解决这个设计缺陷和满足我当时项目开发需要。我稍微对[TextInlineSprite 3.0.0](https://github.com/coding2233/TextInlineSprite/tree/Text-v3.0-b) 进行了调整

* 删除了InlineManager合批渲染相同表情ID的代码。
* 重构表情渲染流程
* 增加单独修改表情大小的代码

**删除了InlineManager合批渲染相同表情ID的代码。**

重要！！重要！！重要！！删除后不支持同时渲染多套表情ID。对于多数游戏项目来讲，只会用到一套表情。删除这些代码的主要目的是提升代码效率。

**重构表情渲染流程**

因为不再对相同的表情ID合批渲染。修改后相应的工作转移到了InlineText。而表情集对象SpriteAsset在InlineText的Inspector面板绑定。如下图
![](https://raw.githubusercontent.com/lizijie/lizijie.github.io/master/assets/images/2019-09-18-%E8%AE%B0%E4%B8%80%E6%AC%A1%E4%BF%AE%E6%94%B9unity3d%E5%AF%8C%E6%96%87%E6%9C%AC%E6%8F%92%E4%BB%B6%E7%9A%84%E8%AE%BE%E8%AE%A1%E7%BC%BA%E9%99%B7/inlinetext_add_feature.png)

InlineText.DealSpriteTagInfo在处理表情顶点时删除了转世界标点的代码。同时UI-Emoji也作了修改后，在这里只需要为顶点赋uv值就可。

![](https://raw.githubusercontent.com/lizijie/lizijie.github.io/master/assets/images/2019-09-18-%E8%AE%B0%E4%B8%80%E6%AC%A1%E4%BF%AE%E6%94%B9unity3d%E5%AF%8C%E6%96%87%E6%9C%AC%E6%8F%92%E4%BB%B6%E7%9A%84%E8%AE%BE%E8%AE%A1%E7%BC%BA%E9%99%B7/inlinetext_deal_sprite.png)


**增加单独修改表情大小的代码**

![](https://raw.githubusercontent.com/lizijie/lizijie.github.io/master/assets/images/2019-09-18-%E8%AE%B0%E4%B8%80%E6%AC%A1%E4%BF%AE%E6%94%B9unity3d%E5%AF%8C%E6%96%87%E6%9C%AC%E6%8F%92%E4%BB%B6%E7%9A%84%E8%AE%BE%E8%AE%A1%E7%BC%BA%E9%99%B7/inlinetext_stick_size_anim.gif)


**关于性能问题**

由于删除InlineManager的合批，Batches会随InlineText数量线性增长。当时我对我们项目中的聊天消息列表，进行了性能测试。

首先要声明一下
1. 列表满1页可容纳8条聊天消息
2. 每条聊天消息有6个表情

* 当聊天信息条数2条（不满1页）时，Batches相同，修改后顶点少0.1k，三角面少0.1k
* 当聊天信息条数8条（刚好满1页）时，修改后的代码多5个Batches，修改后顶点少1k，三角面少0.2k
* 当聊天信息条数29条时（超过1页）时，修改前的代码Batches没有增加。修改后的代码多28个Batches，修改后的顶点少2k，三角面少1k。

**然而batches增长的问题，可以用无限滚动列表组件过来解决，这样聊天消息条数不会超过1页数量，间接控制了batches和顶点的增加。总体上，batches多约5个batches。个人认为性能还可接受。**

以上修改放在我的github上<b><https://github.com/lizijie/TextInlineSprite/tree/m2></b>，欢迎下载！

<br>	
<br>	
<b>原文:<br>
<https://lizijie.github.io/2019/09/18/%E8%AE%B0%E4%B8%80%E6%AC%A1%E4%BF%AE%E6%94%B9unity3d%E5%AF%8C%E6%96%87%E6%9C%AC%E6%8F%92%E4%BB%B6%E7%9A%84%E8%AE%BE%E8%AE%A1%E7%BC%BA%E9%99%B7.html>
<br>
作者github:<br>	
<https://github.com/lizijie>	
</b>
