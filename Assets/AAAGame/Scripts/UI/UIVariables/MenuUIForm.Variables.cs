//---------------------------------
//此文件由工具自动生成,请勿手动修改
//更新自:525105219@qq.com
//更新时间:06/11/2022 21:45:05
//---------------------------------
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public partial class MenuUIForm
{
	private TextMeshProUGUI moneyText = null;
	private Text levelText = null;
	private Text nextLevelText = null;
	private Slider levelProgress = null;
	private Text curLvNumText = null;
	protected override void InitUIProperties()
	{
		var fields = this.GetFieldsProperties();
		moneyText = fields[0].GetComponent<TextMeshProUGUI>(0);
		levelText = fields[1].GetComponent<Text>(0);
		nextLevelText = fields[2].GetComponent<Text>(0);
		levelProgress = fields[3].GetComponent<Slider>(0);
		curLvNumText = fields[4].GetComponent<Text>(0);
	}
}
