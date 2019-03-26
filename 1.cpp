//从给定的数组中找到利润最大的作业序列，有最后期限、利润和花费时间的工作
#include<iostream>
#include<algorithm>//保存了C++中的sort函数
#define min(a,b) ((a)<(b))?(a):(b)

using namespace std;

// Job的结构
struct Job
{
   char id;      // 工作 Id
   int dead;    // 工作的deadline
   int profit; // 在deadline之前完成时，工作的获利
   int cost;  // 工作的cost
   int value;
};

 
 
// 此函数用于根据一天的利润与截止日期对所有作业进行排序，先按value升序排列，如果value相同，再按dead降序排列。
bool comparison(Job a, Job b)
{
	if(a.value!=b.value)
		return (a.value > b.value);
	else
		return (a.dead < b.dead);
}
 
//返回平台所需的最小数目
void printJobScheduling(Job arr[], int n, int max)
{
    // 根据compare函数对所有作业进行排序，Sort(数组开始位置start,数组结束位置end,排序方法：默认从小到大，如果要从大到小排，使用compare函数)
	sort(arr, arr+n, comparison);//即使arr+n写作arr+1也可以排序，说明第二个参数是不参与排序的
	int* result = new int[n]; // 存储结果(作业序列)
    bool* slot = new bool[max];  // 跟踪空闲时间
	int r=0;
 
    // 初始化所有位置为空闲
    for (int i=0; i<max; i++)
        slot[i] = false;
 
    // 遍历所有给定的作业
    for (i=0; i<n; i++)
	{
		int sum_cost=0;
		//已排好的工作消耗的时间和
		if(i>0)
		{
			for(int k=0; k<i; k++)
			{
				sum_cost=sum_cost+arr[k].cost;
			}
		}

		//判断添加的任务是否会与之前任务发生冲突，没有问题则将结果放入result中，标记已占用的时间单位
		if(slot[sum_cost]==false && sum_cost+arr[i].cost<=arr[i].dead)
		{
			result[r]=i;
			r++;
			for(int j=sum_cost; j<sum_cost+arr[i].cost; j++)
			{
				slot[j] = true;
			}
		}
	}


    // 输出答案
    for (i=0; i<n; i++)
       if (slot[i])
         cout << arr[result[i]].id << " ";
}

int main()
{
	int profit,cost;
    int value=profit/cost;
	//依次为id, dead, profit, cost
	Job arr[] = { {'1', 10, 2, 4}, {'2', 8, 3, 3}, {'3', 4, 3, 2},{'4', 2, 6, 1}};
	//确定工作个数
    int n = sizeof(arr)/sizeof(arr[0]);
	//算出所需的最大时间空间
	int max=arr[0].dead;
	for (int i=0;i<n;i++)
		if(max<arr[i].dead)
			max=arr[i].dead;
	//输出结果
    cout << "Following is maximum profit sequence of jobs\n";
    printJobScheduling(arr, n, max);
    return 0;
}

